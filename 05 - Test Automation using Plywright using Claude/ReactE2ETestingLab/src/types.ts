export type Product = {
  id: number;
  name: string;
  price: number;
  category: string;
};

export type CartItem = Product & {
  quantity: number;
};
