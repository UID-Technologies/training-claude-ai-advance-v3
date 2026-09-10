# Trainer Notes

## Recommended Demo Flow

### Exercise 1
Ask Claude to analyze the React application and create an automation test strategy.

### Exercise 2
Generate login tests:
- valid login
- empty credentials
- wrong password

### Exercise 3
Generate product/cart tests:
- search
- add to cart
- quantity
- total

### Exercise 4
Generate one full E2E business journey:

Login
→ Search
→ Add to Cart
→ Checkout
→ Order Confirmation

## Good Teaching Points

- automation test vs E2E test
- stable selectors
- role/text selectors vs data-testid
- avoid `waitForTimeout`
- assert business outcomes
- independent tests
- Claude-generated tests require review
