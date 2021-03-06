﻿// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using System;

/* 'JSSoft.Crema.Runtime.Generation.Cpp (net452)' 프로젝트에서 병합되지 않은 변경 내용
이전:
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
이후:
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
*/
using System.CodeDom.Compiler;

namespace JSSoft.Crema.Runtime.Generation.Cpp.CodeDom
{
    public class NativeCCodeProvider : CodeDomProvider
    {
        [Obsolete]
        public override System.CodeDom.Compiler.ICodeCompiler CreateCompiler()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override System.CodeDom.Compiler.ICodeGenerator CreateGenerator()
        {
            throw new NotImplementedException();
        }

        public override System.CodeDom.Compiler.ICodeGenerator CreateGenerator(System.IO.TextWriter output)
        {
            return new NativeCCodeGenerator();
        }
    }
}
