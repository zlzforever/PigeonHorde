using PigeonHorde.Model;

namespace PigeonHorde.Services;

public class AgentService
{
    public void Register(Service service)
    {
        service.Initialize();

        using var pipe = Connector.Redis.StartPipe();

        Repository.AddService(pipe, service);
        Repository.PublishServiceRegisterEvent(pipe, new ServiceChangedEvent
        {
            Id = service.Id,
            Name = service.Name,
            OperateType = OperateType.Register
        });
        pipe.EndPipe();
    }

    public void Deregister(string serviceId)
    {
        var service = Repository.GetService(serviceId);
        if (service == null)
        {
            return;
        }

        using var pipe = Connector.Redis.StartPipe();
        Repository.RemoveService(pipe, service);
        Repository.PublishServiceRegisterEvent(pipe, new ServiceChangedEvent
        {
            Id = service.Id,
            Name = service.Name,
            OperateType = OperateType.Remove
        });
        pipe.EndPipe();
    }
}