﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using Cassandra;
using Cassandra.Data.Linq;
#if MYTEST
using MyTest;
using System.Threading.Tasks;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cassandra.MSTest;
#endif

namespace Cassandra.Data.Linq.MSTest
{
    [TestClass]
    public class ComplexTests
    {
        private string KeyspaceName = "test";

        Session Session;

        [TestInitialize]
        public void SetFixture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            CCMBridge.ReusableCCMCluster.Setup(2);
            CCMBridge.ReusableCCMCluster.Build(Cluster.Builder());
            Session = CCMBridge.ReusableCCMCluster.Connect("tester");
            Session.CreateKeyspaceIfNotExists(KeyspaceName);
            Session.ChangeKeyspace(KeyspaceName);
        }

        [TestCleanup]
        public void Dispose()
        {
            CCMBridge.ReusableCCMCluster.Drop();
        }

        [TestMethod]
        [WorksForMe]
        public void DoTest()
        {
            Diagnostics.CassandraTraceSwitch.Level = TraceLevel.Verbose;
            CassandraLogWriter LogWriter = new CassandraLogWriter();
            TextWriterTraceListener twtl = new TextWriterTraceListener(LogWriter);

            Trace.Listeners.Add(twtl);


            Console.WriteLine("Connecting, setting keyspace and creating Tables..");
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            LogWriter.GetContext(new CassandraLogContext(Session));

            TwitterContext twitterContext = new TwitterContext(Session);

            var TweetsTable = twitterContext.GetTable<Tweet>();
            var AuthorsTable = twitterContext.GetTable<Author>();
            var FollowedTweetsTable = twitterContext.GetTable<FollowedTweet>();
            var StatisticsTable = twitterContext.GetTable<Statistics>();

            Console.WriteLine("Done!");

            //Adding authors and their followers to the Authors table: 
            Console.WriteLine("Adding authors and their followers to the Authors table..");
            int AuthorsNo = 10;
            List<Author> authorsLocal = new List<Author>();
            List<Statistics> statisticsLocal = new List<Statistics>();
            List<string> authorsID = new List<string>();

            for (int i = 0; i < AuthorsNo; i++)
            {
                var author_ID = "Author" + i.ToString();
                var authorEnt = new Author() { author_id = author_ID, followers = authorsID.Where(aut => aut != author_ID).ToList() };
                AuthorsTable.AddNew(authorEnt);
                AuthorsTable.EnableQueryTracing(authorEnt);
                authorsLocal.Add(authorEnt);
                authorsID.Add(authorEnt.author_id);

                //We will also add current author to the Statistics table: 
                var statEnt = new Statistics() { author_id = author_ID };
                StatisticsTable.Attach(statEnt, EntityUpdateMode.ModifiedOnly, EntityTrackingMode.KeepAttachedAfterSave);

                //And increment number of followers for current author, also in Statistics table: 
                authorEnt.followers.ForEach(folo => statEnt.followers_count += 1);
                statisticsLocal.Add(statEnt);
            }
            twitterContext.SaveChanges(SaveChangesMode.Batch);
            var traces = AuthorsTable.RetriveAllQueryTraces();
            foreach (var trace in traces)
            {
                Console.WriteLine("coordinator was {0}", trace.Coordinator);
            }
            Console.WriteLine("Done!");


            //Now every author will add a single tweet:
            Console.WriteLine("Now authors are writing their tweets..");
            List<Tweet> tweetsLocal = new List<Tweet>();
            List<FollowedTweet> followedTweetsLocal = new List<FollowedTweet>();
            foreach (var auth in authorsID)
            {
                var tweetEnt = new Tweet() { tweet_id = Guid.NewGuid(), author_id = auth, body = "Hello world! My name is " + auth + (DateTime.Now.Second % 2 == 0 ? "." : ""), date = DateTimeOffset.Now };
                TweetsTable.AddNew(tweetEnt);
                tweetsLocal.Add(tweetEnt);

                //We will update our statistics table                 
                statisticsLocal.First(stat => stat.author_id == auth).tweets_count += 1;

                //We also have to add this tweet to "FollowedTweet" table, so every user who follows that author, will get this tweet on his/her own wall!
                FollowedTweet followedTweetEnt = null;
                Author author = AuthorsTable.FirstOrDefault(a => a.author_id == auth).Execute();
                if (author != default(Author) && author.followers != null)
                    foreach (var follower in author.followers)
                    {
                        followedTweetEnt = new FollowedTweet() { user_id = follower, author_id = tweetEnt.author_id, body = tweetEnt.body, date = tweetEnt.date, tweet_id = tweetEnt.tweet_id };
                        FollowedTweetsTable.AddNew(followedTweetEnt);
                        followedTweetsLocal.Add(followedTweetEnt);
                    }
            }
            twitterContext.SaveChanges(SaveChangesMode.Batch);
            Console.WriteLine("Done!");
            string separator = Environment.NewLine + "───────────────────────────────────────────────────────────────────────" + Environment.NewLine;

            Console.WriteLine(separator);

            //To display users that follows "Author8":         
            Console.WriteLine("\"Author8\" is followed by:" + Environment.NewLine);
            try
            {
                Author Author8 = AuthorsTable.First(aut => aut.author_id == "Author8").Execute();
                Author8.displayFollowers();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("\"Author8\" does not exist in database!");
            }


            //To display all of user "Author15" tweets:
            Console.WriteLine(separator + "All tweets posted by Author15:" + Environment.NewLine);
            foreach (Tweet tweet in (from twt in TweetsTable where twt.author_id == "Author15" select twt).Execute())
                tweet.display();


            //To display all tweets from users that "Author45" follows:
            string author_id = "Author45";
            Console.WriteLine(separator + string.Format("All tweets posted by users that \"{0}\" follows:", author_id) + Environment.NewLine);

            // At first we will check if specified above author_id is present in database:
            Author specifiedAuthor = (from aut in AuthorsTable where aut.author_id == author_id select aut).FirstOrDefault().Execute(); // it's another possible way of using First/FirstOrDefault method 

            if (specifiedAuthor != default(Author))
            {
                var followedTweets = (from t in FollowedTweetsTable where t.user_id == author_id select t).Execute().ToList();

                if (followedTweets.Count() > 0)
                    foreach (var foloTwt in followedTweets)
                        foloTwt.display();
                else
                    Console.WriteLine(string.Format("There is no tweets from users that {0} follows.", author_id));
            }
            else
                Console.WriteLine(string.Format("Nothing to display because specified author: \"{0}\" does not exist!", author_id));


            //Let's check all of authors punctuation in their tweets, or at least, if they end their tweets with full stop, exclamation or question mark:
            List<string> authorsWithPunctuationProblems = new List<string>();

            //To check it, we can use anonymous class because we are interested only in author_id and body of the tweet
            var slimmedTweets = (from twt in TweetsTable select new { twt.author_id, twt.body }).Execute();
            foreach (var slimTwt in slimmedTweets)
                if (!(slimTwt.body.EndsWith(".") || slimTwt.body.EndsWith("!") || slimTwt.body.EndsWith("?")))
                    if (!authorsWithPunctuationProblems.Contains(slimTwt.author_id))
                        authorsWithPunctuationProblems.Add(slimTwt.author_id);

            if (authorsWithPunctuationProblems.Count > 0)
            {
                // Now we can check how many of all authors have this problem..            
                float proportion = (float)authorsWithPunctuationProblems.Count() / AuthorsTable.Count().Execute() * 100;
                Console.WriteLine(separator + string.Format("{0}% of all authors doesn't end tweet with punctuation mark!", proportion) + Environment.NewLine);

                // This time I will help them, and update these tweets with a full stop..            
                foreach (var tweet in TweetsTable.Where(x => authorsWithPunctuationProblems.Contains(x.author_id)).Execute())
                {
                    TweetsTable.Attach(tweet);
                    tweetsLocal.Where(twt => twt.tweet_id == tweet.tweet_id).First().body += ".";
                    tweet.body += ".";
                }
                twitterContext.SaveChanges(SaveChangesMode.Batch);
            }


            //Statistics before deletion of tweets:
            Console.WriteLine(separator + "Before deletion of all tweets our \"Statistics\" table looks like:" + Environment.NewLine);
            StatisticsTable.DisplayTable();


            //Deleting all tweets from "Tweet" table
            foreach (var ent in tweetsLocal)
            {
                TweetsTable.Delete(ent);

                var statEnt = statisticsLocal.FirstOrDefault(auth => auth.author_id == ent.author_id);
                if (statEnt != default(Statistics))
                    statEnt.tweets_count -= 1;

            }
            twitterContext.SaveChanges(SaveChangesMode.Batch);

            //Statistics after deletion of tweets:
            Console.WriteLine("After deletion of all tweets our \"Statistics\" table looks like:");
            StatisticsTable.DisplayTable();

            //Logs:
            Console.WriteLine(separator + "Number of received logs: " + LogWriter.LogsTable.Count().Execute());
            foreach (var log in LogWriter.LogsTable.Execute())
                log.display();

            LogWriter.StopWritingToDB();

        }

    }

    public static class Utils
    {
        public static void DisplayTable(this Table<Statistics> table)
        {
            Console.WriteLine("┌──────────┬────────────────┬─────────────┐");
            Console.WriteLine(String.Format("│{0,10}│{1,16}│{2,13}│", "Author ID", "Followers count", "Tweets count"));
            Console.WriteLine("├──────────┼────────────────┼─────────────┤");

            foreach (var stat in (from st in table select st).Execute())
                Console.WriteLine(String.Format("│{0,10}│{1,16}│{2,13}│", stat.author_id, stat.followers_count, stat.tweets_count));

            Console.WriteLine("└──────────┴────────────────┴─────────────┘");
        }
    }
}
