export interface AuditLogDTO {
  id: number;
  userId?: number | null;
  username: string;
  action: string;
  entityName: string;
  entityId?: string | null;
  description: string;
  createdDate: string;
}

export interface AuditLogFilterDTO {
  keyword?: string;
  action?: string;
  pageIndex: number;
  pageSize: number;
}
