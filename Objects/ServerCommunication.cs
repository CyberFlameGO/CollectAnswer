using CollectAnswers.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls;

namespace CollectAnswers.Objects
{
    class ServerCommunication
    {

        public async static System.Threading.Tasks.Task<String> tryToRegisterUser(string username, PasswordBox password)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("registration") },
                { "username", JsonValue.CreateStringValue(username) },
                { "password", JsonValue.CreateStringValue(password.Password) },
            }.Stringify();

            string response = await sendAndAwaitForResponse("registration", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToLoginUser(string username, PasswordBox password)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("login") },
                { "username", JsonValue.CreateStringValue(username) },
                { "password", JsonValue.CreateStringValue(password.Password) },
            }.Stringify();

            string response = await sendAndAwaitForResponse("login", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToLike(int postId, int textType, bool value)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("reaction") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "textType", JsonValue.CreateStringValue(textType + "") },
                { "postId", JsonValue.CreateStringValue(postId + "") },
                { "value", JsonValue.CreateStringValue(value + "") },
            }.Stringify();

            string response = await sendAndAwaitForResponse("reaction", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToPost(string text)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("post") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "text", JsonValue.CreateStringValue(text) },
            }.Stringify();

            string response = await sendAndAwaitForResponse("post", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToPostComment(int postId, string text)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("comment") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "postId", JsonValue.CreateStringValue(postId + "") },
                { "text", JsonValue.CreateStringValue(text) },
            }.Stringify();

            string response = await sendAndAwaitForResponse("comment", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToGetPosts(int lastPostId)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("getposts") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "lastPostId", JsonValue.CreateStringValue(lastPostId + "") },
            }.Stringify();

            string response = await sendAndAwaitForResponse("getposts", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToGetSearchPosts(int lastPostId, String words)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("getsearchposts") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "lastPostId", JsonValue.CreateStringValue(lastPostId + "") },
                { "words", JsonValue.CreateStringValue(words) },
            }.Stringify();

            string response = await sendAndAwaitForResponse("getsearchposts", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToGetPostPerId(int postId)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("getpostsperid") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "postId", JsonValue.CreateStringValue(postId + "") },
            }.Stringify();

            string response = await sendAndAwaitForResponse("getpostsperid", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToGetComments(int postId, int lastCommentId)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("getcomments") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "postId", JsonValue.CreateStringValue(postId + "") },
                { "lastCommentId", JsonValue.CreateStringValue(lastCommentId + "") },
            }.Stringify();

            string response = await sendAndAwaitForResponse("getcomments", input);

            return response;
        }

        public async static System.Threading.Tasks.Task<String> tryToGetCommentPerId(int postId, int commentId)
        {
            string input = new JsonObject
            {
                { "type", JsonValue.CreateStringValue("getcommentssperid") },
                { "username", JsonValue.CreateStringValue(LocalDatabase.username) },
                { "userId", JsonValue.CreateStringValue("" + LocalDatabase.userId) },
                { "token", JsonValue.CreateStringValue(LocalDatabase.token) },
                { "postId", JsonValue.CreateStringValue(postId + "") },
                { "commentId", JsonValue.CreateStringValue(commentId + "") },
            }.Stringify();

            string response = await sendAndAwaitForResponse("getcommentssperid", input);

            return response;
        }

        private async static System.Threading.Tasks.Task<String> sendAndAwaitForResponse(string type, string jsonString)
        {
            string response = new JsonObject
            {
                { "type", JsonValue.CreateStringValue(type) },
                { "status", JsonValue.CreateStringValue("error") }
            }.Stringify();

            IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse("51.116.180.248"), 1025);

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(serverAddress);

                // Sending
                int toSendLen = System.Text.Encoding.UTF8.GetByteCount(jsonString);
                byte[] toSendBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
                byte[] toSendLenBytes = System.BitConverter.GetBytes(toSendLen);
                clientSocket.Send(toSendLenBytes);
                clientSocket.Send(toSendBytes);

                // Receiving
                byte[] rcvLenBytes = new byte[4];
                clientSocket.Receive(rcvLenBytes);
                int rcvLen = System.BitConverter.ToInt32(rcvLenBytes, 0);
                byte[] rcvBytes = new byte[rcvLen];
                clientSocket.Receive(rcvBytes);
                String rcv = System.Text.Encoding.UTF8.GetString(rcvBytes);

                response = rcv;

                clientSocket.Close();
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                if (clientSocket.Connected)
                    clientSocket.Close();
            }
            return response;
        }
    }
}
