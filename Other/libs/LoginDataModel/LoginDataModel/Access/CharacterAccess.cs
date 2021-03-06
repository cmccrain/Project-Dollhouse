﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoginDataModel.Entities
{
    public class CharacterAccess
    {
        private DataAccess Context;

        public CharacterAccess(DataAccess context)
        {
            this.Context = context;
        }

        public IQueryable<Character> GetForAccount(int accountId)
        {
            return Context.Context.Characters.Where(x => x.AccountID == accountId);
        }

        public CharacterCreationStatus CreateCharacter(Character Char)
        {
            if (Char.Name.Length > 24)
            {
                return CharacterCreationStatus.ExceededCharacterLimit;
            }

            try
            {
                Context.Context.Characters.InsertOnSubmit(Char);
                Context.Context.SubmitChanges();
            }
            catch (Exception)
            {
                return CharacterCreationStatus.NameAlreadyExisted;
            }

            return CharacterCreationStatus.Success;
        }

        public void RetireCharacter(Character Char)
        {
            Context.Context.Characters.DeleteOnSubmit(Char);
            Context.Context.SubmitChanges();
        }
    }

    public enum CharacterCreationStatus
    {
        NameAlreadyExisted,
        ExceededCharacterLimit,
        Success,
        GeneralError
    }
}
