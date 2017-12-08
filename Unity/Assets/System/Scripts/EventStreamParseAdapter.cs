using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EventStreamParserAdapter : IParser
{
    private readonly IEnumerator<ParsingEvent> enumerator;

    public EventStreamParserAdapter(IEnumerable<ParsingEvent> events)
    {
        enumerator = events.GetEnumerator();
    }

    public ParsingEvent Current
    {
        get
        {
            return enumerator.Current;
        }
    }

    public bool MoveNext()
    {
        return enumerator.MoveNext();
    }
}
