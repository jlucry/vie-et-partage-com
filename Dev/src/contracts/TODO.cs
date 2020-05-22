using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    public class TODO
    {
        //BUG: Data: Les cats Audio et Film sont inversé!!!
        //BUG: Html: Les Videos youtube ne sont pas affiché!!!
        //BUG: Data: La date de fin d'event est éronné pour Pele Mars (7 jour de durés)

        //TODO: DEV: File upload https://github.com/danialfarid/ng-file-upload/blob/master/README.md
        //TODO: DEV: Garder l'image de base + l'image cropé : Crop Img: https://github.com/alexk111/ngImgCrop

        // Post edit page:
        // TODO: Gérer l'encodage des données texte (titre, text, etc, ...).
        // TODO: Mettre à jour le date picker.
        // TODO: Post text edition: add video from youtube using summernote add video features.

        //TODO: Add tooltips on all buttons.

        // Publication:
        // TODO: Ajouter un etat autre que la validation qui permet aux autres contributeurs d'ajouter des fichiers.
        //       Quand le post est publié aucun element ne peu etre ajouter.

        //TODO: Overwrite the base staticFIle middlewar to manage access to files.

        // Interface de gestion:
        //  * Exposer des fonctions de gestions.
        //  * Controler l'access aux fonctions de gestions.
        //  * Authentifier l'utilsateur.
        // Gestions des pages:
        //  * Lister les pages (avec hierachies et filtrage).
        //      * Filtrer les pages
        //  * Visualiser une page.
        //  * Ajouter une page.
        //  * Modifier une page.
        //  * Effacer une page.
    }
}
