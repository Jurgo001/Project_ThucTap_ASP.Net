using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace ProductCrud.DataServices.Audit;

public class AuditLogQueue : IAuditLogQueue
{
    private readonly Channel<AuditLogMessage> _channel;

    public AuditLogQueue()
    {
        _channel = Channel.CreateUnbounded<AuditLogMessage>();
    }

    public ValueTask EnqueueAsync(
        AuditLogMessage message,
        CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(
            message,
            cancellationToken);
    }

    public IAsyncEnumerable<AuditLogMessage> ReadAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(
            cancellationToken);
    }
}
