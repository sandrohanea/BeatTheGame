using System;
using System.Collections.Generic;

namespace BeatTheGame.Data
{
    public record Hand(Guid PlayerId, List<Card> Cards);
}
