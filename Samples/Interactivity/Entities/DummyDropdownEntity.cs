//
//  DummyDropdownEntity.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace Remora.Discord.Samples.Interactivity.Entities;

/// <summary>
/// Defines an interactive entity with a dropdown.
/// </summary>
public class DummyDropdownEntity : InMemoryPersistentInteractiveEntity<IReadOnlyList<Embed>>, ISelectMenuInteractiveEntity
{
    private readonly InteractionContext _context;
    private readonly IDiscordRestChannelAPI _channelAPI;

    /// <summary>
    /// Initializes a new instance of the <see cref="DummyDropdownEntity"/> class.
    /// </summary>
    /// <param name="context">The interaction context.</param>
    /// <param name="channelAPI">The channel API.</param>
    public DummyDropdownEntity(
        InteractionContext context,
        IDiscordRestChannelAPI channelAPI
    )
    {
        _context = context;
        _channelAPI = channelAPI;
    }

    /// <inheritdoc />
    public override string Nonce => _context.Message.IsDefined(out var message)
        ? message.ID.ToString()
        : throw new InvalidOperationException();

    /// <inheritdoc />
    public override Task<Result<bool>> IsInterestedAsync
    (
        ComponentType? componentType,
        string customID,
        CancellationToken ct = default
    )
    {
        return componentType is not ComponentType.SelectMenu
            ? Task.FromResult<Result<bool>>(false)
            : Task.FromResult<Result<bool>>(customID is "dummy-dropdown");
    }

    /// <inheritdoc />
    public async Task<Result> HandleInteractionAsync(
        IUser user,
        string customID,
        IReadOnlyList<string> values,
        CancellationToken ct = default
    )
    {
        if (!_context.Message.IsDefined(out var message))
        {
            return new InvalidOperationError("Interaction without a message?");
        }

        if (values.Count != 1)
        {
            return new InvalidOperationError("Only one element may be selected at any one time.");
        }

        var indexRaw = values.Single();
        if (!int.TryParse(indexRaw, out var index))
        {
            return new ParsingError<int>(indexRaw);
        }

        var embed = this.Data[index];

        return (Result)await _channelAPI.EditMessageAsync
        (
            _context.ChannelID,
            message.ID,
            embeds: new[] { embed },
            ct: ct
        );
    }
}
