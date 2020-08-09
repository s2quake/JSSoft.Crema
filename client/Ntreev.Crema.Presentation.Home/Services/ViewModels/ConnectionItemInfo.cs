﻿//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Ntreev.Crema.Presentation.Home.Services.ViewModels
{
    public struct ConnectionItemInfo
    {
        [XmlElement]
        public string Version { get; set; }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string Address { get; set; }

        [XmlElement]
        public string DataBaseName { get; set; }

        [XmlElement]
        public string ID { get; set; }

        [XmlElement]
        public string Password { get; set; }

        [XmlElement]
        public Color ThemeColor { get; set; }

        [XmlElement]
        public string Theme { get; set; }

        [XmlElement]
        public bool IsDefault { get; set; }

        [XmlElement]
        public DateTime LastConnectedDateTime { get; set; }

        public static readonly ConnectionItemInfo Empty = new ConnectionItemInfo()
        {
            ThemeColor = FirstFloor.ModernUI.Presentation.AppearanceManager.Current.AccentColor,
        };
    }
}
