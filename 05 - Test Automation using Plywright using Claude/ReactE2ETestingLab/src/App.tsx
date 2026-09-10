import { useMemo, useState } from 'react';
import { products } from './data';
import type { CartItem, Product } from './types';

type View = 'login' | 'shop' | 'cart' | 'checkout' | 'success';

function App() {
  const [view, setView] = useState<View>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loginError, setLoginError] = useState('');
  const [search, setSearch] = useState('');
  const [cart, setCart] = useState<CartItem[]>([]);
  const [checkoutError, setCheckoutError] = useState('');
  const [orderNumber, setOrderNumber] = useState('');

  const filteredProducts = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) return products;
    return products.filter(p =>
      p.name.toLowerCase().includes(term) ||
      p.category.toLowerCase().includes(term)
    );
  }, [search]);

  const cartTotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);

  function login() {
    setLoginError('');
    if (!email || !password) {
      setLoginError('Email and password are required');
      return;
    }

    if (email !== 'student@example.com' || password !== 'Password123!') {
      setLoginError('Invalid credentials');
      return;
    }

    setView('shop');
  }

  function addToCart(product: Product) {
    setCart(current => {
      const existing = current.find(i => i.id === product.id);
      if (existing) {
        return current.map(i =>
          i.id === product.id ? { ...i, quantity: i.quantity + 1 } : i
        );
      }
      return [...current, { ...product, quantity: 1 }];
    });
  }

  function updateQuantity(id: number, quantity: number) {
    if (quantity <= 0) {
      setCart(current => current.filter(i => i.id !== id));
      return;
    }
    setCart(current =>
      current.map(i => i.id === id ? { ...i, quantity } : i)
    );
  }

  function placeOrder(form: HTMLFormElement) {
    const data = new FormData(form);
    const fullName = String(data.get('fullName') || '').trim();
    const address = String(data.get('address') || '').trim();
    const city = String(data.get('city') || '').trim();
    const pincode = String(data.get('pincode') || '').trim();

    setCheckoutError('');

    if (!fullName || !address || !city || !pincode) {
      setCheckoutError('All checkout fields are required');
      return;
    }

    if (!/^\d{6}$/.test(pincode)) {
      setCheckoutError('Pincode must be exactly 6 digits');
      return;
    }

    if (cart.length === 0) {
      setCheckoutError('Cart is empty');
      return;
    }

    setOrderNumber(`ORD-${Date.now().toString().slice(-6)}`);
    setCart([]);
    setView('success');
  }

  return (
    <main className="container">
      <header className="topbar">
        <div>
          <h1>ShopSmart</h1>
          <p>React Automation & E2E Testing Lab</p>
        </div>

        {view !== 'login' && (
          <nav>
            <button onClick={() => setView('shop')}>Products</button>
            <button onClick={() => setView('cart')}>
              Cart ({cart.reduce((n, i) => n + i.quantity, 0)})
            </button>
          </nav>
        )}
      </header>

      {view === 'login' && (
        <section className="card" data-testid="login-page">
          <h2>Login</h2>
          <label>
            Email
            <input
              data-testid="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="student@example.com"
            />
          </label>
          <label>
            Password
            <input
              data-testid="password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="Password123!"
            />
          </label>
          {loginError && <p className="error" role="alert">{loginError}</p>}
          <button data-testid="login-button" onClick={login}>Login</button>
          <p className="hint">
            Demo user: student@example.com / Password123!
          </p>
        </section>
      )}

      {view === 'shop' && (
        <section data-testid="shop-page">
          <div className="toolbar">
            <h2>Products</h2>
            <input
              data-testid="search"
              placeholder="Search products..."
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </div>

          {filteredProducts.length === 0 && (
            <p data-testid="no-products">No products found</p>
          )}

          <div className="grid">
            {filteredProducts.map(product => (
              <article className="card" key={product.id} data-testid={`product-${product.id}`}>
                <h3>{product.name}</h3>
                <p>{product.category}</p>
                <strong>₹{product.price}</strong>
                <button
                  data-testid={`add-${product.id}`}
                  onClick={() => addToCart(product)}
                >
                  Add to Cart
                </button>
              </article>
            ))}
          </div>
        </section>
      )}

      {view === 'cart' && (
        <section data-testid="cart-page">
          <h2>Your Cart</h2>

          {cart.length === 0 ? (
            <p data-testid="empty-cart">Your cart is empty</p>
          ) : (
            <>
              {cart.map(item => (
                <article className="cart-row" key={item.id}>
                  <div>
                    <strong>{item.name}</strong>
                    <p>₹{item.price}</p>
                  </div>
                  <input
                    aria-label={`Quantity for ${item.name}`}
                    type="number"
                    min="0"
                    value={item.quantity}
                    onChange={e => updateQuantity(item.id, Number(e.target.value))}
                  />
                </article>
              ))}

              <h3 data-testid="cart-total">Total: ₹{cartTotal}</h3>

              <button data-testid="checkout-button" onClick={() => setView('checkout')}>
                Proceed to Checkout
              </button>
            </>
          )}
        </section>
      )}

      {view === 'checkout' && (
        <section className="card" data-testid="checkout-page">
          <h2>Checkout</h2>

          <form
            onSubmit={e => {
              e.preventDefault();
              placeOrder(e.currentTarget);
            }}
          >
            <label>
              Full Name
              <input name="fullName" data-testid="fullName" />
            </label>

            <label>
              Address
              <input name="address" data-testid="address" />
            </label>

            <label>
              City
              <input name="city" data-testid="city" />
            </label>

            <label>
              Pincode
              <input name="pincode" data-testid="pincode" />
            </label>

            {checkoutError && <p className="error" role="alert">{checkoutError}</p>}

            <button type="submit" data-testid="place-order">
              Place Order
            </button>
          </form>
        </section>
      )}

      {view === 'success' && (
        <section className="card success" data-testid="success-page">
          <h2>Order Confirmed</h2>
          <p>Your order has been placed successfully.</p>
          <strong data-testid="order-number">{orderNumber}</strong>
          <button onClick={() => setView('shop')}>Continue Shopping</button>
        </section>
      )}
    </main>
  );
}

export default App;
