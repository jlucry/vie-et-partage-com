using Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Models
{
    /// <summary>
    /// Post registration field.
    /// </summary>
    public class PostRegistrationField
    {
        /// <summary>
        /// Cons
        /// </summary>
        public PostRegistrationField()
        {
        }

        /// <summary>
        /// Field UId.
        /// </summary>
        public string UId { get; set; }

        /// <summary>
        /// Field title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Field position.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Field type.
        /// 1: Champ de saisie.
        /// 2: Champ de choix oui\non.
        /// 3: Champ de choix customizable.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Field Choose.
        /// </summary>
        public string[] Choose { get; set; }

        /// <summary>
        /// Field Choose 2.
        /// </summary>
        public string[] Choose2 { get; set; }

        /// <summary>
        /// Details in case of yes\no choose.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Post default registration fields.
        /// </summary>
        public static PostRegistrationField[] DefaultRegistrationFields =
        {
            new PostRegistrationField
            {
                UId = "situation",
                Title = "Votre situation",
                Position = 1,
                Type = 5,
                Mandatory = true,
                Choose = new string[] { "Femme", "Homme" },
                Choose2 = new string[] { "Célibatair(e)", "En couple", "Fiancé(e)", "Marié(e)", "Prêtre", "Religieux" }
            },
            new PostRegistrationField
            {
                UId = "firstname",
                Title = "Votre nom",
                Position = 2,
                Type = 1,
                Mandatory = true
            },
            new PostRegistrationField
            {
                UId = "lastname",
                Title = "Votre prénom",
                Position = 3,
                Type = 1,
                Mandatory = true
            },
            new PostRegistrationField
            {
                UId = "age",
                Title = "Votre âge",
                Position = 4,
                Type = 1,
                Mandatory = true
            },
            new PostRegistrationField
            {
                UId = "city",
                Title = "Votre ville de résidence",
                Position = 5,
                Type = 1,
                Mandatory = true
            },
            new PostRegistrationField
            {
                UId = "phone",
                Title = "Votre téléphone",
                Position = 6,
                Type = 1,
                Mandatory = true
            },
            new PostRegistrationField
            {
                UId = "mail",
                Title = "Votre e-mail",
                Position = 7,
                Type = 1,
                Mandatory = true
            },
            new PostRegistrationField
            {
                UId = "medical",
                Title = "Avez vous des problèmes médicaux et\\ou suivez vous un régime alimentaire particulier ?",
                Position = 8,
                Type = 3,
                Details = "Si oui précisez \\ nom de(s) maladie(s) \\ régimes alimentaires"
            },
            new PostRegistrationField
            {
                UId = "first",
                Title = "Est-ce votre première retraite de ce type ?",
                Position = 9,
                Type = 3,
                Details = "Si oui indiquez les noms et prénoms des personnes qui vous ont invités"
            },
            new PostRegistrationField
            {
                UId = "question",
                Title = "Avez des questions \\ remarques complementaires ?",
                Position = 10,
                Type = 3,
                Details = "Si oui indiquez"
            },
        };
    }
}