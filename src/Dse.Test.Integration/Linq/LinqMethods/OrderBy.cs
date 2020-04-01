//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Collections.Generic;
using System.Linq;
using Dse.Data.Linq;
using Dse.Test.Integration.Linq.Structures;
using Dse.Test.Integration.TestClusterManagement;
using Dse.Mapping;
using Dse.Test.Integration.SimulacronAPI.PrimeBuilder.Then;
using NUnit.Framework;

#pragma warning disable 612

namespace Dse.Test.Integration.Linq.LinqMethods
{
    public class OrderBy : SimulacronTest
    {
        private List<Movie> _movieList = Movie.GetDefaultMovieList();
        private readonly string _uniqueKsName = TestUtils.GetUniqueKeyspaceName();
        private Table<Movie> _movieTable;

        public override void SetUp()
        {
            base.SetUp();
            MappingConfiguration movieMappingConfig = new MappingConfiguration();
            movieMappingConfig.MapperFactory.PocoDataFactory.AddDefinitionDefault(typeof(Movie),
                () => LinqAttributeBasedTypeDefinition.DetermineAttributes(typeof(Movie)));
            _movieTable = new Table<Movie>(Session, movieMappingConfig);
            Session.ChangeKeyspace(_uniqueKsName);
        }

        [Test]
        public void LinqOrderBy()
        {
            List<Movie> moreMovies = new List<Movie>();
            string sameTitle = "sameTitle";
            string sameMovieMaker = "sameMovieMaker";
            for (int i = 0; i < 10; i++)
            {
                Movie movie = Movie.GetRandomMovie();
                movie.Title = sameTitle;
                movie.MovieMaker = sameMovieMaker;
                moreMovies.Add(movie);
            }
            List<Movie> expectedOrderedMovieList = moreMovies.OrderBy(m => m.Director).ToList();
            TestCluster.PrimeFluent(
                b => b.WhenQuery(
                          "SELECT \"director\", \"list\", \"mainGuy\", \"movie_maker\", \"unique_movie_title\", \"yearMade\" " +
                          $"FROM \"{Movie.TableName}\" WHERE \"unique_movie_title\" = ? AND \"movie_maker\" = ? ALLOW FILTERING",
                          rows => rows.WithParams(sameTitle, sameMovieMaker))
                      .ThenRowsSuccess(Movie.CreateRowsResult(expectedOrderedMovieList)));

            var movieQuery = _movieTable.Where(m => m.Title == sameTitle && m.MovieMaker == sameMovieMaker);

            List<Movie> actualOrderedMovieList = movieQuery.Execute().ToList();
            Assert.AreEqual(expectedOrderedMovieList.Count, actualOrderedMovieList.Count);
            for (int i = 0; i < expectedOrderedMovieList.Count; i++)
            {
                Assert.AreEqual(expectedOrderedMovieList[i].Director, actualOrderedMovieList[i].Director);
                Assert.AreEqual(expectedOrderedMovieList[i].MainActor, actualOrderedMovieList[i].MainActor);
                Assert.AreEqual(expectedOrderedMovieList[i].MovieMaker, actualOrderedMovieList[i].MovieMaker);
            }
        }

        [Test]
        public void LinqOrderBy_Unrestricted_Sync()
        {
            TestCluster.PrimeFluent(
                b => b.WhenQuery(
                          "SELECT \"director\", \"list\", \"mainGuy\", \"movie_maker\", \"unique_movie_title\", \"yearMade\" " +
                          $"FROM \"{Movie.TableName}\" ORDER BY \"mainGuy\" ALLOW FILTERING")
                      .ThenServerError(ServerError.Invalid, "ORDER BY is only supported when the partition key is restricted by an EQ or an IN."));

            try
            {
                _movieTable.OrderBy(m => m.MainActor).Execute();
                Assert.Fail("Expected Exception was not thrown!");
            }
            catch (InvalidQueryException e)
            {
                Assert.AreEqual("ORDER BY is only supported when the partition key is restricted by an EQ or an IN.", e.Message);
            }
        }

        [Test]
        public void LinqOrderBy_Unrestricted_Async()
        {
            TestCluster.PrimeFluent(
                b => b.WhenQuery(
                          "SELECT \"director\", \"list\", \"mainGuy\", \"movie_maker\", \"unique_movie_title\", \"yearMade\" " +
                          $"FROM \"{Movie.TableName}\" ORDER BY \"mainGuy\" ALLOW FILTERING")
                      .ThenServerError(ServerError.Invalid, "ORDER BY is only supported when the partition key is restricted by an EQ or an IN."));
            var ex = Assert.ThrowsAsync<InvalidQueryException>(
                async () => await _movieTable.OrderBy(m => m.MainActor).ExecuteAsync().ConfigureAwait(false));
            const string expectedException = "ORDER BY is only supported when the partition key is restricted by an EQ or an IN.";
            Assert.AreEqual(expectedException, ex.Message);
        }
    }
}