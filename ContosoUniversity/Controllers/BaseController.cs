using System;
using System.Web.Mvc;
using ContosoUniversity.Services;
using ContosoUniversity.Models;
using ContosoUniversity.Data;
using Microsoft.Extensions.Logging;

namespace ContosoUniversity.Controllers
{
    public abstract class BaseController : Controller
    {
        protected SchoolContext db;
        protected NotificationService notificationService = new NotificationService();
        protected readonly ILogger _logger;

        public BaseController()
        {
            db = SchoolContextFactory.Create();
            // Each derived controller creates its own typed logger via the factory.
            // BaseController itself gets a logger under its own category so that
            // infrastructure-level log messages (e.g. notification failures) are
            // distinguishable from controller-specific ones.
            _logger = LoggingService.CreateLogger<BaseController>();
        }

        protected void SendEntityNotification(string entityType, string entityId, EntityOperation operation)
        {
            SendEntityNotification(entityType, entityId, null, operation);
        }

        protected void SendEntityNotification(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            try
            {
                var userName = "System"; // No authentication, use System as default user
                notificationService.SendNotification(entityType, entityId, entityDisplayName, operation, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send {Operation} notification for {EntityType} '{EntityId}' (displayName={EntityDisplayName})",
                    operation, entityType, entityId, entityDisplayName);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
                notificationService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

