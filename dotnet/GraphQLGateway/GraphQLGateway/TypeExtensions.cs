using HotChocolate.Language;

namespace GraphQLGateway;

public class QueryTypeExtension : ObjectTypeExtension {
    protected override void Configure(IObjectTypeDescriptor descriptor) {
        descriptor.Name("Query");
        // descriptor.Field("conversations")
        //     .Ignore();
    }
}

//public class ConversationTypeExtension : ObjectTypeExtension {
//    protected override void Configure(IObjectTypeDescriptor descriptor) {
//        descriptor.Name("Conversation");
//        descriptor.Field("participants")
//            .Directive("delegate",
//                new ArgumentNode("schema", "UserApi"),
//                new ArgumentNode("path", "nodes(ids: $fields.participants)"))
//            .Type("[User]!");
//    }
//}

//public class MessageTypeExtension : ObjectTypeExtension {
//    protected override void Configure(IObjectTypeDescriptor descriptor) {
//        descriptor.Name("Message");
//        descriptor.Field("sentBy")
//            .Directive("delegate",
//                new ArgumentNode("schema", "UserApi"),
//                new ArgumentNode("path", "user(id: $fields.sentBy)"))
//            .Type("User!");
//    }
//}



