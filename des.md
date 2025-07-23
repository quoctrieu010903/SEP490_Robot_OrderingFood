List products

```json

{
  "message": "string",
  "data": [
    {
      "id": "string",
      "name": "string",
      "price": "string",
      "urlImg": "string"
    }
    ...
  ],
  "HttpStatus": ""
}
```

=> Price = gía của size bé nhất

Production Detail

```json
{
  "message": "string",
  "data": {
    "id": "string",
    "name": "string",
    "urlImg": "string",
    "description": "string",
    "price": "string",
    "sizes": [
      {
        "id": "string",
        "name": "string",
        "price": "string"
      },
      {
        "id": "string",
        "name": "string",
        "price": "string"
      },
      ...
    ]
  },
  "HttpStatus": ""
}

```

list topping

```json
{
  "message": "string",
  "data": [
    {
      "id": "string",
      "name": "string",
      "price": "string",
      "urlImg": "string"
    },
    {
      "id": "string",
      "name": "string",
      "price": "string",
      "urlImg": "string"
    },
    ...
  ],
  "HttpStatus": ""
}

```

# order

### request :

param:

```
"id":"string", => id table
```

body:

```json
{
  "id": "string",
  =>
  id
  Table
  "idSize": "string",
  =>
  id
  Size
  "toppings": [
    {
      "id": "string"
      =>
      id
      topping
    }
  ]
}
```

### repont:

```json
{
  "message": "string",
  "data": {
    "id": "string",
    "status": "Status"
  },
  "HttpStatus": ""
}
```

=> api nay co the dung cho vo hang va ca thanh toan ngay.

# Mua ngay

goi api order +> lay id va gui ve api mua ngay

- request:

```json
{
  "idTable": "string",
  "orders": [
    {
      "id": "string"
    },{
      "id": "string"
    },...
  ],
  "PaymentMethor": "PaymentMethor"
}
```

- repont:

```json
{
  "message": "string",
  "data": "string",
  "HttpStatus": ""
}
```

=> data : tra ve theo phuong thuc thanh toans

- nếu thanh toán tiền mặt thì sẽ trả về 1 note thông báo.
- nếu chuyển khoản thì gửi về ủrl của qr code.

# mua nhiu mon

- requet:

```json
{
  "idtable": "string",
  "idOrders": [
    {
      "id": "string"
    },
    {
      "id": "string"
    },
    {
      "id": "string"
    },
    ...
  ],
  "PaymentMethor": "Paymentmethor"
}
```

- repont:

```json
{
  "message": "string",
  "data": "string",
  "HttpStatus": ""
}
```

# Danh sach order

```json
{
  "message": "string",
  "data": [
    {
      "id": "string",
      "name": "string",
      "totolPrice": "string",
      "timeOrder": "string",
      "timeExpected": "string",
      "nameTopping": [
        "string",
        "string",
        ...
      ],
      "statusOrder": "statusOrder",
      "PaymentStatus": "PaymentStatus"
    },
    {}
    ....
  ],
  "PaymentMethor": "Paymentmethor"
}
```

# yeu cau thanh toan:

- requet:
  param:

```
id: string => id table
```

param:

```
PaymantStatus: PaymantStatus => phuong thuc thanh toan
```

 
