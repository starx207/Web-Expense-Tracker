/*

    This example code taken from http://blogs.clariusconsulting.net/kzu/making-extension-methods-amenable-to-mocking/

 */




// someType.Messaging().SendTo(address); 




// public static class MessagingExtensions
// {
//     public static IMessaging Messaging(this SomeType target)
//     {
//         return MessagingFactory(target);
//     }

//     static MessagingExtensions()
//     {
//         MessagingFactory = st => new Messaging(st);
//     }

//     internal static Func MessagingFactory { get; set; }
// }








// public interface IMessaging
// {
//     void SendTo(string address);
// }

// internal class Messaging : IMessaging
// {
//     SomeType someType;

//     public Messaging(SomeType someType)
//     {
//         this.someType = someType;
//     }

//     public void SendTo(string address)
//     {
//         // Do something with someType and the address.
//     }
// }










// var mockMessaging = new Mock();
// MessagingExtensions.MessagingFactory = st => mockMessaging.Object;