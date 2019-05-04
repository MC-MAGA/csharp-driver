﻿// 
//       Copyright (C) 2019 DataStax Inc.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 

using Dse.Responses;
using Moq;

namespace Dse.Test.Unit.Requests
{
    internal class ProxyResultResponse : ResultResponse
    {
        public ProxyResultResponse(ResultResponseKind kind) : base(kind, Mock.Of<IOutput>())
        {
        }

        public ProxyResultResponse(ResultResponseKind kind, IOutput output) : base(kind, output)
        {
        }
    }
}