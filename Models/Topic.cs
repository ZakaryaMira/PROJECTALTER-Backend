﻿using System;
using System.Collections.Generic;

namespace PROJECTALTERAPI.Models;

public partial class Topic
{
    public long TopicId { get; set; }

    public long UserId { get; set; }

    public string TopicName { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
