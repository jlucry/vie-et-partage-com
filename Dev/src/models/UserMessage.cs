using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class UserMessage
    {
        public const string InternalError = "Une erreur s'est produite veuillez réessayer ultérieurement!";
        public const string UnexpectedError = "Une erreur s'est produite veuillez réessayer ultérieurement!";
        public const string InvalidInputs = "Veuillez verifier vos saisies!";
        public const string AccessDenied = "Accès refusé!";
        public const string AccessDeniedAlreadyValidated = "Accès refusé, post déjà validé!";
		
		public const string UserNotEnabled = "L'administrateur du site n'a pas encore activé votre compte, veuillez réessayer ultérieurement!";
		public const string PasswordsDontMatch = "Les mots de passe ne correspondent pas!";
		public const string UserRequiresTwoFactor = "Authentification double activée.";
		public const string UserLockedOut = "Nous n'avons pas pu vous connecter. Votre compte est bloqué!";
		public const string InvalidUserEmailPassword = "Nous n'avons pas pu vous connecter. L'adresse e-mail ou le mot de passe fourni sont incorrects!";
    }
}
