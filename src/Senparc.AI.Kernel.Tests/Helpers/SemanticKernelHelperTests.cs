﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.Kernel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.AI.Kernel.Tests.Helpers
{
    [TestClass]
    public class SemanticKernelHelperTests
    {
        [TestMethod]
        public void GetKernelTest()
        {
            var helper = new SemanticKernelHelper();
            var kernel = helper.GetKernel();    
            Assert.IsNotNull(kernel);
        }

    }
}
