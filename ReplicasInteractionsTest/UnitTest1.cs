using System;
using System.Collections.Generic;
using DadStormServices;
using NUnit.Framework;
using Moq;
using ReplicaProcess;
using ReplicaProcess.Routing;

namespace ReplicasInteractionsTest
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void ReroutingPrimaryFailure()
        {
            // OP1: 0
            // OP2: (0, 1) primary
            // Crash OP2(0)
            // OP1 -> OP2
            // Assert OP1(0) -> OP2(1)

            var op2_0 = new Mock<IDownstreamReplica>();
            var op2_1 = new Mock<IDownstreamReplica>();
            var op2_2 = new Mock<IDownstreamReplica>();

            var op1_op2 = new DownstreamOperator("OP2", new List<IDownstreamReplica> { op2_0.Object, op2_1.Object, op2_2.Object }, new PrimaryRoutingStrategy());

            //op2_0.Setup(r => r.Send(It.IsAny<IList<IList<string>>>(), It.IsAny<IList<TupleId>>(), It.IsAny<bool>()))
            //    .Returns(false);
            //op2_1.Setup(r => r.Send(It.IsAny<IList<IList<string>>>(), It.IsAny<IList<TupleId>>(), It.IsAny<bool>()))
            //    .Returns(true);

            //op1_op2.Flow(new DadTuple(new TupleId("OP1", 0, 0), new List<string> { "a", "b" }), false);

            //op2_1.Verify(r => r.Send(It.IsAny<IList<IList<string>>>(), It.IsAny<IList<TupleId>>(), It.IsAny<bool>()));
            //op2_2.Verify(r => r.Send(It.IsAny<IList<IList<string>>>(), It.IsAny<IList<TupleId>>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
