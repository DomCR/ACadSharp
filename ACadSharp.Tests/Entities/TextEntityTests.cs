using System;
using ACadSharp.Entities;
using CSMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
    public class TextEntityTests
    {

        [Fact]
        public void RotationDoesNotOverrideAlignmentPoint()
        {
            var text = new TextEntity
            {
                Rotation = Math.PI
            };
            Assert.Equal(text.AlignmentPoint, XYZ.Zero);
        }
    }
}
