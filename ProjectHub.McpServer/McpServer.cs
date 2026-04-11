using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHub.McpServer
{
    public class McpServer
    {
        private readonly StdioTransport _transport;
        private readonly Dictionary<string, Action<Dictionary<string, object>, Action<Dictionary<string, object>>>> _tools;

        public McpServer()
        {
            _transport = new StdioTransport();
            _tools = new Dictionary<string, Action<Dictionary<string, object>, Action<Dictionary<string, object>>>>();
            RegisterTools();
        }

        private void RegisterTools()
        {
            _tools["list_projects"] = HandleListProjects;
            _tools["open_project"] = HandleOpenProject;
            _tools["search_projects"] = HandleSearchProjects;
            _tools["get_recent_projects"] = HandleGetRecentProjects;
            _tools["add_project"] = HandleAddProject;
        }

        public async Task StartAsync()
        {
            await _transport.StartAsync(HandleRequest);
        }

        private async Task HandleRequest(string requestJson)
        {
            try
            {
                var request = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(requestJson);
                if (request != null && request.ContainsKey("toolcall"))
                {
                    var toolCall = request["toolcall"] as Dictionary<string, object>;
                    if (toolCall != null && toolCall.ContainsKey("name"))
                    {
                        var toolName = toolCall["name"] as string;
                        var arguments = toolCall.ContainsKey("args") ? toolCall["args"] as Dictionary<string, object> : new Dictionary<string, object>();

                        if (_tools.ContainsKey(toolName))
                        {
                            _tools[toolName](arguments, response =>
                            {
                                var responseJson = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    tool_result = new
                                    {
                                        name = toolName,
                                        result = response
                                    }
                                });
                                _transport.SendResponse(responseJson);
                            });
                        }
                        else
                        {
                            SendErrorResponse("Tool not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SendErrorResponse(ex.Message);
            }
        }

        private void SendErrorResponse(string error)
        {
            var responseJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = new
                {
                    message = error
                }
            });
            _transport.SendResponse(responseJson);
        }

        private void HandleListProjects(Dictionary<string, object> args, Action<Dictionary<string, object>> callback)
        {
            // 模拟实现
            callback(new Dictionary<string, object>
            {
                { "projects", new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            { "id", "uuid-1" },
                            { "name", "电商平台重构" },
                            { "alias", "主站" },
                            { "path", "D:\\projects\\ecommerce" },
                            { "tags", new List<string> { "vue", "frontend" } },
                            { "defaultIde", "VS Code" }
                        }
                    }
                },
                { "total", 1 }
            });
        }

        private void HandleOpenProject(Dictionary<string, object> args, Action<Dictionary<string, object>> callback)
        {
            // 模拟实现
            callback(new Dictionary<string, object>
            {
                { "success", true },
                { "message", "已使用 VS Code 打开项目「电商平台重构」" },
                { "project", new Dictionary<string, object>
                    {
                        { "id", "uuid-1" },
                        { "name", "电商平台重构" },
                        { "path", "D:\\projects\\ecommerce" }
                    }
                },
                { "ide", "VS Code" }
            });
        }

        private void HandleSearchProjects(Dictionary<string, object> args, Action<Dictionary<string, object>> callback)
        {
            // 模拟实现
            callback(new Dictionary<string, object>
            {
                { "projects", new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            { "id", "uuid-1" },
                            { "name", "电商平台重构" },
                            { "alias", "主站" },
                            { "path", "D:\\projects\\ecommerce" },
                            { "tags", new List<string> { "vue", "frontend" } }
                        }
                    }
                }
            });
        }

        private void HandleGetRecentProjects(Dictionary<string, object> args, Action<Dictionary<string, object>> callback)
        {
            // 模拟实现
            callback(new Dictionary<string, object>
            {
                { "projects", new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            { "id", "uuid-1" },
                            { "name", "电商平台重构" },
                            { "path", "D:\\projects\\ecommerce" },
                            { "lastOpenedAt", DateTime.UtcNow.ToString() }
                        }
                    }
                }
            });
        }

        private void HandleAddProject(Dictionary<string, object> args, Action<Dictionary<string, object>> callback)
        {
            // 模拟实现
            callback(new Dictionary<string, object>
            {
                { "success", true },
                { "message", "项目添加成功" },
                { "project", new Dictionary<string, object>
                    {
                        { "id", "uuid-2" },
                        { "name", "新项目" },
                        { "path", args.ContainsKey("path") ? args["path"] : "" }
                    }
                }
            });
        }
    }
}