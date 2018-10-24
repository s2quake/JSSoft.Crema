﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Data;

namespace Ntreev.Crema.Services.Extensions
{
    public static class TypeTemplateExtensions
    {
        public static Task<bool> ContainsAsync(this ITypeTemplate template, string memberName)
        {
            return template.Dispatcher.InvokeAsync(() => template.Contains(memberName));
        }
    }
}
