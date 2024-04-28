using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat.S.MAUI;


public struct MediaFile
{
    public byte[] data;
    public string contentType;
}
public static class Media
{

    public static SortedList<string, MediaFile> Dic {  get; private  set; }
    static Task init;

    

    static Media()
    {
        Dic = [];
        init = Init();


    }

    static async Task Init()
    {
        HttpListenerContext context;
        // Crear instancia de HttpListener y configurarla
        HttpListener listener = new();
        listener.Prefixes.Add("http://localhost:8080/"); // Puedes cambiar el puerto y la dirección según tus necesidades
        listener.Start();
        Console.WriteLine("Servidor iniciado. Esperando solicitudes...");

        while (true)
        {
            // Esperar la próxima solicitud
            context = await listener.GetContextAsync();

            // Procesar la solicitud en un subproceso para no bloquear el bucle principal
            _ = Task.Run(() => HandleRequest(context));
        }
    }
    static async Task HandleRequest(HttpListenerContext context)
    {
        MediaFile file;
        // Obtener el objeto de respuesta
        var response = context.Response;
        // Obtenemos la URL referente
        string? uuid = context.Request.RawUrl?.Trim('/');

        // Verificamos si la URL referente no es nula
      
        try
        {
            if (!string.IsNullOrEmpty(uuid) && Dic.ContainsKey(uuid))
            {
                file = Dic[uuid];
                // Configurar encabezados de respuesta
                response.ContentType = file.contentType;
                response.ContentLength64 = file.data!.Length;

                // Escribir los bytes del archivo en la respuesta
                await response.OutputStream.WriteAsync(file.data.AsMemory(0, file.data.Length));
            }
        }
        catch (Exception ex)
        {
            // Manejar cualquier error
            Console.WriteLine($"Error al manejar la solicitud: {ex.Message}");
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            // Finalizar la respuesta
            response.Close();
        }
     
            
        
    }
    public static string GetFileId(FileResult file)
    {
        return GetFileId(File.ReadAllBytes(file.FullPath), file.ContentType);
    }

    public static string GetFileId(byte[] file,string contentType)
    {
        string id=Guid.NewGuid().ToString();

        Dic.Add(id,new MediaFile() { contentType = contentType,data=file });

        return id;
    }

    public static void FromBytes(this MediaElement mediaElement,byte[] file,string contentType)
    {
        string id=GetFileId(file,contentType);
        mediaElement.Source = $"http://localhost:8080/{id}";
    }
    public static void FromFile(this MediaElement mediaElement, FileResult file)
    {
        mediaElement.FromBytes(File.ReadAllBytes(file.FullPath), file.ContentType);
    }

    public static void FromId(this MediaElement mediaElement, string id)
    {
        if (Dic.ContainsKey(id))
        {
            mediaElement.Source = $"http://localhost:8080/{id}";
        }
    }




}
