using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AndroidAzure
{
	[Activity(Label = "AndroidAzure", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		ImageView ImagenDrop;
		string archivoLocal;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Main);
			Button btnImagen = FindViewById<Button>(Resource.Id.btnrealizar);
			ImagenDrop = FindViewById<ImageView>
				(Resource.Id.imagen);
			btnImagen.Click += ArchivoImagen;
		}
		async void ArchivoImagen(object sender, EventArgs e)
		{
            try
            {
                var ruta = await DescargaImagen();
                Android.Net.Uri rutaImagen = Android.Net.Uri.Parse(ruta);
                ImagenDrop.SetImageURI(rutaImagen);

                CloudStorageAccount cuentaAlmacenamiento =
                    CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=tallerxamarin;AccountKey=s+A8siTK0j504BTPkIBUT3e05t2OBoddrEXTkBMAbk1gEOH3ry7Vcs0ROAA0CPwfd9xL57Y1ywim+i+nDUNV5w==");
                CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                CloudBlobContainer contenedor = clienteBlob.GetContainerReference("lab1");
                CloudBlockBlob recursoblob = contenedor.GetBlockBlobReference(archivoLocal);
                await recursoblob.UploadFromFileAsync(ruta);

                Toast.MakeText(this, "Guardado en Azure Storage Blob", ToastLength.Long).Show();

                CloudTableClient tableClient = cuentaAlmacenamiento.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("Ubicaciones");

                await table.CreateIfNotExistsAsync();

                UbicacionEntity ubica = new UbicacionEntity(archivoLocal, "México");
                ubica.Latitud = 21.152216;
                ubica.Localidad = "León";
                ubica.Longitud = -101.711537;

                TableOperation insertar = TableOperation.Insert(ubica);
                await table.ExecuteAsync(insertar);

                Toast.MakeText(this, "Guardado en Azure Storage Table NoSQL", ToastLength.Long).Show();
            }
            catch (Exception exc)
            {
                Toast.MakeText(this, exc.Message, ToastLength.Long).Show();
            }

		}
		public async Task<string> DescargaImagen()
		{

			WebClient client = new WebClient();
            byte[] imageData = await client.DownloadDataTaskAsync("https://dl.dropboxusercontent.com/u/95408124/foto1.jpg");
        
            string documentspath = System.Environment.GetFolderPath
				(System.Environment.SpecialFolder.Personal);
			archivoLocal = "foto1.jpg";
			string localpath = Path.Combine(documentspath, archivoLocal);
			File.WriteAllBytes(localpath, imageData);
			return localpath;
		}
	}
	public class UbicacionEntity : TableEntity
	{
		public UbicacionEntity(string Archivo, string Pais)
		{
			this.PartitionKey = Archivo;
			this.RowKey = Pais;
		}
		public UbicacionEntity() { }
		public double Latitud { get; set; }
		public double Longitud { get; set; }
		public string Localidad { get; set; }
	}

}