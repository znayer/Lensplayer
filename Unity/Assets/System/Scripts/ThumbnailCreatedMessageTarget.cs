﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ThumbnailCreatedMessageTarget : IEventSystemHandler
{
    void ThumbnailCreated(string videoFilename, string thumbnailFilename);
}