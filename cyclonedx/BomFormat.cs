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
namespace CycloneDX.CLI
{
    public enum BomFormat
    {
        Unsupported,
        Xml,
        Xml_v1_0,
        Xml_v1_1,
        Xml_v1_2,
        Xml_v1_3,
        Json,
        Json_v1_2,
        Json_v1_3,
        Protobuf,
        Protobuf_v1_3,
        Csv,
        SpdxTag,
        SpdxTag_v2_1,
        SpdxTag_v2_2
    }
}
