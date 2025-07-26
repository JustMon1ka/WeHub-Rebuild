using MessageService.Controllers;
using MessageService.DTOs;
using MessageService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MessageService.Tests
{
    public class MessagesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<IMessageRepository> _messageRepositoryMock;

        public MessagesControllerTests(WebApplicationFactory<Program> factory)
        {
            _messageRepositoryMock = new Mock<IMessageRepository>();
            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // 替换实际的 IMessageRepository 为 Mock
                        services.AddScoped(_ => _messageRepositoryMock.Object);
                    });
                })
                .CreateClient();

            // 设置 JWT 认证头
            var token = GenerateJwtToken("1"); // 假设当前用户ID为1
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task GetConversations_ReturnsOkWithConversations()
        {
            // Arrange
            var conversations = new List<ConversationDto>
            {
                new ConversationDto
                {
                    OtherUserId = 2,
                    LastMessage = new MessageDto { MessageId = 1, SenderId = 1, ReceiverId = 2, Content = "Hello", SentAt = DateTime.UtcNow, IsRead = false },
                    UnreadCount = 1
                }
            };
            _messageRepositoryMock.Setup(repo => repo.GetConversationsAsync(1)).ReturnsAsync(conversations);

            // Act
            var response = await _client.GetAsync("/api/messages");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<ConversationDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Single(result);
            Assert.Equal(2, result[0].OtherUserId);
            Assert.Equal("Hello", result[0].LastMessage.Content);
        }

        [Fact]
        public async Task GetMessages_ReturnsOkWithMessages()
        {
            // Arrange
            var messages = new List<MessageDto>
            {
                new MessageDto { MessageId = 1, SenderId = 1, ReceiverId = 2, Content = "Hi", SentAt = DateTime.UtcNow, IsRead = false }
            };
            _messageRepositoryMock.Setup(repo => repo.GetMessagesAsync(1, 2)).ReturnsAsync(messages);

            // Act
            var response = await _client.GetAsync("/api/messages/2");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<MessageDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Single(result);
            Assert.Equal("Hi", result[0].Content);
        }

        [Fact]
        public async Task SendMessage_ValidInput_ReturnsCreated()
        {
            // Arrange
            var messageDto = new SendMessageDto { Content = "Test message" };
            var expectedMessage = new MessageDto
            {
                MessageId = 1,
                SenderId = 1,
                ReceiverId = 2,
                Content = "Test message",
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _messageRepositoryMock.Setup(repo => repo.AddMessageAsync(It.IsAny<MessageService.Models.Message>()))
                .Callback<MessageService.Models.Message>(m => m.MessageId = 1);
            _messageRepositoryMock.Setup(repo => repo.GetMessagesAsync(1, 2)).ReturnsAsync(new List<MessageDto> { expectedMessage });

            // Act
            var content = new StringContent(JsonSerializer.Serialize(messageDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/messages/2", content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MessageDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal("Test message", result.Content);
        }

        [Fact]
        public async Task SendMessage_EmptyContent_ReturnsBadRequest()
        {
            // Arrange
            var messageDto = new SendMessageDto { Content = "" };
            var content = new StringContent(JsonSerializer.Serialize(messageDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/messages/2", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task MarkAsRead_ReturnsNoContent()
        {
            // Arrange
            _messageRepositoryMock.Setup(repo => repo.MarkAsReadAsync(1, 2)).Returns(Task.CompletedTask);

            // Act
            var response = await _client.PutAsync("/api/messages/2/read", null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }

        private string GenerateJwtToken(string userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userId)
            };
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secure-secret-key-for-jwt-authentication-1234567890"));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: "your-issuer",
                audience: "your-audience",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);
            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}