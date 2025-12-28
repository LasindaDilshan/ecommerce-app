export interface RecentPurchase {
  productName: string;
  customerName: string;
  location: string;
  purchaseTime: Date;
  timeAgo: string;
}

export interface ProductSocialProof {
  productId: number;
  totalSold: number;
  soldLast24Hours: number;
  currentViewers: number;
  recentPurchases: RecentPurchase[];
}
