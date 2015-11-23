// -----------------------------------------------------------------------
// <copyright file="CharacterAttributeUtil.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using PathfinderDb.Schema;

namespace WikiExportParser.Wiki
{
    internal static class CharacterAttributeUtil
    {
        public static CharacterAttribute FromText(string text)
        {
            if (text.Length == 3)
            {
                switch (text.ToLowerInvariant())
                {
                    case CharacterAttributes.Strength:
                        return CharacterAttribute.Strength;
                    case CharacterAttributes.Dexterity:
                        return CharacterAttribute.Dexterity;
                    case CharacterAttributes.Constitution:
                        return CharacterAttribute.Constitution;
                    case CharacterAttributes.Intelligence:
                        return CharacterAttribute.Intelligence;
                    case CharacterAttributes.Wisdom:
                        return CharacterAttribute.Wisdom;
                    case CharacterAttributes.Charisma:
                        return CharacterAttribute.Charisma;
                    // Spécifique français
                    case "for":
                        return CharacterAttribute.Strength;
                    case "sag":
                        return CharacterAttribute.Wisdom;
                }
            }

            throw new NotSupportedException(string.Format("Impossible de traduire le texte {0} en un attribut", text));
        }

        public static string AsText(CharacterAttribute value)
        {
            switch (value)
            {
                case CharacterAttribute.Charisma:
                    return CharacterAttributes.Charisma;
                case CharacterAttribute.Constitution:
                    return CharacterAttributes.Constitution;
                case CharacterAttribute.Dexterity:
                    return CharacterAttributes.Dexterity;
                case CharacterAttribute.Intelligence:
                    return CharacterAttributes.Intelligence;
                case CharacterAttribute.Strength:
                    return CharacterAttributes.Strength;
                case CharacterAttribute.Wisdom:
                    return CharacterAttributes.Wisdom;
                default:
                    return string.Empty;
            }
        }
    }
}