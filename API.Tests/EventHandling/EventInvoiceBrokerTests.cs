namespace API.Tests.EventHandling; 

using FluentAssertions;
using API.EventHandling;
using Moq;
using NUnit.Framework;

internal class EventInvoiceBrokerTests
{
    [Test]
    public void EventInvoiceBroker_Subscribe_Publish_ShouldReceiveEvent()
    {
        //Arrange
        var data = new EventInvoiceData(123, 23, "Example");
        var subscriberMock = new Mock<EventHandler<EventInvoiceData>>();
        var target = new EventInvoiceBroker();

        //Act
        target.Subscribe(subscriberMock.Object);

        target.Publish(data);

        //Assert
        subscriberMock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
    }

    [Test]
    public void EventInvoiceBroker_MultipleSubscribers_Publish_ShouldAllReceiveEvent()
    {
        //Arrange
        var data = new EventInvoiceData(123, 23, "Example");
        var subscriber1Mock = new Mock<EventHandler<EventInvoiceData>>();
        var subscriber2Mock = new Mock<EventHandler<EventInvoiceData>>();
        var subscriber3Mock = new Mock<EventHandler<EventInvoiceData>>();
        var target = new EventInvoiceBroker();

        //Act
        target.Subscribe(subscriber1Mock.Object);
        target.Subscribe(subscriber2Mock.Object);
        target.Subscribe(subscriber3Mock.Object);

        target.Publish(data);

        //Assert
        subscriber1Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
        subscriber2Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
        subscriber3Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
    }

    [Test]
    public void EventInvoiceBroker_Subscribe_Unsubscribe_Publish_ShouldNotReceiveEvent()
    {
        //Arrange
        var data = new EventInvoiceData(123, 23, "Example");
        var subscriberMock = new Mock<EventHandler<EventInvoiceData>>();
        var target = new EventInvoiceBroker();

        //Act
        target.Subscribe(subscriberMock.Object);
        target.Unsubscribe(subscriberMock.Object);
        target.Publish(data);

        //Assert
        subscriberMock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Never);
    }

    [Test]
    public void EventInvoiceBroker_MultipleSubscribers_OneUnsubscribes_Publish_ShouldAllReceiveEventExceptOne()
    {
        //Arrange
        var data = new EventInvoiceData(123, 23, "Example");
        var subscriber1Mock = new Mock<EventHandler<EventInvoiceData>>();
        var subscriber2Mock = new Mock<EventHandler<EventInvoiceData>>();
        var subscriber3Mock = new Mock<EventHandler<EventInvoiceData>>();
        var target = new EventInvoiceBroker();

        //Act
        target.Subscribe(subscriber1Mock.Object);
        target.Subscribe(subscriber2Mock.Object);
        target.Subscribe(subscriber3Mock.Object);

        target.Unsubscribe(subscriber2Mock.Object);

        target.Publish(data);

        //Assert
        subscriber1Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
        subscriber2Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Never);
        subscriber3Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
    }

    [Test]
    public void EventInvoiceBroker_Subscribe_PublishTwoTimes_ShouldReceiveTwoEvents()
    {
        //Arrange
        var data1 = new EventInvoiceData(123, 23, "Example1");
        var data2 = new EventInvoiceData(321, 32, "Example2");
        var subscriberMock = new Mock<EventHandler<EventInvoiceData>>();
        var target = new EventInvoiceBroker();

        //Act
        target.Subscribe(subscriberMock.Object);

        target.Publish(data1);
        target.Publish(data2);

        //Assert
        subscriberMock.Verify(x => x.Invoke(It.IsAny<object>(), data1), Times.Once);
        subscriberMock.Verify(x => x.Invoke(It.IsAny<object>(), data2), Times.Once);
    }

    [Test]
    public void EventInvoiceBroker_NoSubscribers_Publish_NoExceptionThrow()
    {
        //Arrange
        var data = new EventInvoiceData(123, 23, "Example");
        var target = new EventInvoiceBroker();

        //Act
        target.Publish(data);

        //Assert
        // No exceptions should be thrown, and no notifications should occur when there are no subscribers
    }

    [Test]
    public void EventInvoiceBroker_MultipleSubscribers_Publish_TwoThrowException_ShouldAllBeInvokedAndExceptionsAgreggated()
    {
        //Arrange
        var data = new EventInvoiceData(123, 23, "Example");
        var subscriber1Mock = new Mock<EventHandler<EventInvoiceData>>();
        var subscriber2Mock = new Mock<EventHandler<EventInvoiceData>>();
        var subscriber3Mock = new Mock<EventHandler<EventInvoiceData>>();
        var target = new EventInvoiceBroker();

        subscriber1Mock.Setup(x => x.Invoke(It.IsAny<object>(), data)).Throws(new Exception());
        subscriber3Mock.Setup(x => x.Invoke(It.IsAny<object>(), data)).Throws(new Exception());

        //Act
        target.Subscribe(subscriber1Mock.Object);
        target.Subscribe(subscriber2Mock.Object);
        target.Subscribe(subscriber3Mock.Object);

        Action act = () => target.Publish(data);

        //Assert
        act.Should().Throw<AggregateException>().Which.InnerExceptions.Should().HaveCount(2);
        subscriber1Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
        subscriber2Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
        subscriber3Mock.Verify(x => x.Invoke(It.IsAny<object>(), data), Times.Once);
    }
}
