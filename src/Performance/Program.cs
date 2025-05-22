// See https://aka.ms/new-console-template for more information

namespace Performance;

static class Program
{
    // 控制并发数量的信号量
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(500);

    // 共享的HttpClient实例
    private static readonly HttpClient HttpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        Console.WriteLine("开始并发请求 API...");

        // 记录开始时间
        var startTime = DateTime.Now;
        var count = 50000;
        var tasks = new Task[count];
        for (var i = 0; i < count; i++)
        {
            var taskId = i;
            tasks[i] = Task.Run(async () => await MakeApiRequest(taskId));
        }

        // 等待所有任务完成
        await Task.WhenAll(tasks);

        // 记录结束时间并计算总耗时
        var endTime = DateTime.Now;
        var totalSeconds = (endTime - startTime).TotalSeconds;

        Console.WriteLine($"所有请求已完成， 总耗时: {totalSeconds:F2}秒, {count / totalSeconds:F4} QPS");
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    static async Task MakeApiRequest(int taskId)
    {
        // 等待信号量，控制并发数量
        await Semaphore.WaitAsync();

        try
        {
            // 替换为实际的API地址
            var apiUrl = "http://127.0.0.1:8500/v1/health/service/test-api";

            // 记录请求开始时间
            var requestStartTime = DateTime.Now;

            // 发送HTTP请求
            var response = await HttpClient.GetAsync(apiUrl);

            // 记录请求结束时间
            var requestEndTime = DateTime.Now;

            // 计算请求耗时
            var requestTime = (requestEndTime - requestStartTime).TotalMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"任务#{taskId} - 成功 (状态码: {response.StatusCode}, 耗时: {requestTime:F0}ms)");
            }
            else
            {
                Console.WriteLine($"任务#{taskId} - 失败 (状态码: {response.StatusCode}, 耗时: {requestTime:F0}ms)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"任务#{taskId} - 异常: {ex.Message}");
        }
        finally
        {
            // 释放信号量
            Semaphore.Release();
        }
    }
}