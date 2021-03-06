﻿using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class CloseTabEvent : PubSubEvent<string> {}
    
    public class UpdateTabStatusEvent : PubSubEvent<TabFlag> {}

}
