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
namespace CycloneDX.CLI.Commands
{
    public enum ConvertOutputFormat
    {
        autodetect,
        xml,
        xml_v1_0,
        xml_v1_1,
        xml_v1_2,
        xml_v1_3,
        json,
        json_v1_2,
        json_v1_3,
        csv,
        spdxtag,
        spdxtag_v2_1,
        spdxtag_v2_2
    }
}
