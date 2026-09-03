using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCrud.DataServices.Audit;

public interface IAuditLogQueue
{
    ValueTask EnqueueAsync(
        AuditLogMessage message,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<AuditLogMessage> ReadAllAsync(
        CancellationToken cancellationToken = default);
}
