export interface DashboardStats {
  totalRevenue: number;
  totalOrders: number;
  totalCustomers: number;
  totalProducts: number;
  pendingOrders: number;
  lowStockProducts: number;
  recentOrders: RecentOrder[];
  topProducts: TopProduct[];
  revenueByMonth: RevenueByMonth[];
}

export interface RecentOrder {
  orderId: number;
  orderNumber: string;
  customerName: string;
  totalAmount: number;
  status: string;
  orderDate: Date;
}

export interface TopProduct {
  productId: number;
  productName: string;
  totalSold: number;
  revenue: number;
}

export interface RevenueByMonth {
  month: string;
  revenue: number;
  orders: number;
}
