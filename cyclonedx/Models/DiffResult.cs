// This file is part of CycloneDX CLI Tool
//
// Licensed under the Apache License, Version 2.0 (the “License”);
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an “AS IS” BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// SPDX-License-Identifier: Apache-2.0
// Copyright (c) Patrick Dwyer. All Rights Reserved.
using System.Collections.Generic;
using CycloneDX.Models.v1_3;
using CycloneDX.Utils;

namespace CycloneDX.CLI.Models
{
    public class DiffResult
    {
        // default to nulls. A value, even if it is empty is to indicate that this option has been invoked.
        public Dictionary<string,DiffItem<Component>> ComponentVersions { get; set; } = null;
    }
}
