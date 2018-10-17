using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace xamCognitive
{
    public partial class MainPage : ContentPage
    {
        private readonly IFaceServiceClient _faceServiceClient;
        ObservableCollection<Face> list = new ObservableCollection<Face>();

        public MainPage()
        {
            InitializeComponent();

            _faceServiceClient = new FaceServiceClient("a7cff5bb619b4b3e8df3dd14eefcb57c", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");

            list = new ObservableCollection<Face>();
            FacesListView.ItemsSource = list;
        }

        private async void TakePicture(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);

            if (!CrossMedia.Current.IsTakePhotoSupported || !CrossMedia.Current.IsCameraAvailable)
            {
                await DisplayAlert("Ops", "Nenhuma câmera detectada.", "OK");

                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(
                new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    Directory = "Demo"
                });

            if (file == null)
                return;

            await Facedetect(file.AlbumPath);

            MinhaImagem.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;

            });
        }

        private async Task Facedetect(string albumPath)
        {
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
                    FaceAttributeType.Gender,
                    FaceAttributeType.Age,
                    FaceAttributeType.Smile,
                    FaceAttributeType.Emotion,
                    FaceAttributeType.Glasses,
                };

            list.Clear();

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(albumPath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imageFileStream,
                       returnFaceId: true,
                       returnFaceLandmarks: false,
                       returnFaceAttributes: faceAttributes);

                    //Add Faces in List
                    foreach (var face in faces)
                    {
                        list.Add(face);
                    }

                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                await DisplayAlert("Error", f.ErrorMessage, "ok");
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "ok");
            }
        }
    }
}
