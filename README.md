# NetCaseStudy

## Ne yaptım ;

* Ürün & sipariş CRUD + kimlik yönetimi (JWT ile giriş/kayıt).
* **Katmanlı mimari** (Api / Application / Domain / Infrastructure) + **test projesi** (**xunit kullandım**).
* **API Versioning** (v1) ve **Swagger/OpenAPI** arayüzü.
* **Validation** (FluentValidation), **CQRS** (MediatR), **AutoMapper**.
* **SQL Server** üzerinde **EF Core** ile veritabanı ve migration.
* **Gelişmiş loglama**: **Serilog** ile dosyaya yapılandırılmış loglar (**NetCaseStudy.Api** projesindeki **Logs** klasörü).
* **Caching**: Dağıtık cache olarak **Redis** (ICacheService → RedisCacheService).
* **ETag & Conditional GET**: Ürün listesinde değişmeyen veri için **304 Not Modified**.
* **Rate Limiting**: Role göre limit (Admin rahat; User için ör. dakikada 50).
* **Policy/resource-tabanlı authorization** (örn. `ViewOrder` politikası + `OrderAuthorizationHandler`).
* **Cursor-based pagination**: Büyük listelerde performans için ayrı cursor endpoint’i.
* **Localization**: **tr-TR** ve **en-US** dil desteği — **NetCaseStudy.Api** projesinde **Resources** içinde **.resx** dosyaları duruyor.

---

## Kullandığım teknolojiler ;

* **.NET**: **.NET 9**
* **ASP.NET Core Web API**, **ASP.NET Identity**
* **EF Core (SqlServer)** + **Migrations**
* **MediatR (CQRS)**, **AutoMapper**
* **FluentValidation**
* **Serilog** (+ File sink) — `Logs/log-YYYYMMDD.txt` dosyaları otomatik oluşuyor
  (örnek: `NetCaseStudy.Api/Logs/log-20250901.txt`)
* **API Versioning** + ApiExplorer/Swagger
* **Health Checks** (SQL Server ve Redis)
* **Redis** (StackExchange.Redis + Microsoft.Extensions.Caching.StackExchangeRedis)

---

## Mimari / Katmanlar ; 

* **Api**: Controller’lar, filtreler, middleware’ler, DI kayıtları, rate limit, auth, swagger vb.
  Örneğin `ETagFilterAttribute` ve `OrdersController` içindeki policy/resource-based kontroller burada.
* **Application**: CQRS (komut/sorgu), DTO’lar, mapping profilleri, arayüzler (örneğin. `ICacheService`).
* **Domain**: Entity’ler, temel iş kuralları yer alıyor. Tüm tablolarda IAudit entity kullandım. (Isactive, Isdeleted, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy gibi.. )
* **Infrastructure**: EF Core DbContext/migrations, `RedisCacheService`, Identity seed vs burada yeralıyor.
* **Tests**: API ve Application testleri; `CustomWebApplicationFactory`, `FakeCacheService`, `FakeCurrentUserService` gibi yardımcılar da burada.

---

## Veritabanı ; 

* **SQL Server** kullandım (`DefaultConnection`). `UseSqlServer(...)` ile bağladım, health check’ler de açık bıraktım.
* Migration ve context **NetCaseStudy.Infrastructure** içinde. Migrations assembly de burada.

---

## Redis ;

* `ICacheService` arayüzü üzerinden erişiyorum; implementasyon **`RedisCacheService`**.
  DI’de **`AddStackExchangeRedisCache`** + **`ConnectionMultiplexer`** ile bağlanıyor.
* Ürün listeleme gibi GET’lerde parametrelere göre key üretiyorum, cache’liyorum.

---

## Loglama ;

* **Serilog** ile structured log alıyorum. Host tarafında `UseSerilog(...ReadFrom.Configuration...)` var.
  Çalıştıkça **`NetCaseStudy.Api/Logs`** altında `log-YYYYMMDD.txt` dosyaları düşüyor.

---

## Authorization ;

* Roller: **Admin/User**.
* **Policy/resource-based** yetki: `ViewOrder` politikası + `OrderAuthorizationHandler` ile bir siparişi sadece sahibi veya yetkili roller görür.

---

## Rate limiting ;

* Rol bazlı limit var: **Admin** çok geniş, **User** sabit pencerede **50 req/dk** gibi .
* Değerleri Api projesindeki return RateLimitPartition kısmından değiştirebiliyorum.

---

## ETag ;

* Ürün listesi endpoint’inde `ETagFilter` ile hash üretiyorum. Veri değişmediyse **304 Not Modified** dönüyor.
* İstemci tarafında ilk GET’te dönen `ETag`’i saklayıp sonraki istekte `If-None-Match` başlığına koyduğumuz zaman 304 aldığımız görülüyor.

---

## API Versioning - Swagger ;

* API **v1** altında çalışıyor.
* **Swagger UI** açık; JWT token’ını sağ üstte Bearer olarak girildikten sonra endpoitnler rahatlıkla test edilebilir.

---

## Localization (tr-TR / en-US) ;

* **en-US** ve **tr-TR** kültürlerini mevcut.
* **NetCaseStudy.Api/Resources** klasöründe **.resx** dosyaları mevcut.
* Denemek için isteklere `Accept-Language: tr-TR` veya `en-US` başlığı eklenebilir veya swagger üzerinden de verilebilir.
* `AddLocalization` + `UseRequestLocalization` ile pipeline’a dahil, doğrulama/mesajlar kültüre göre dönecektir.

---

## Kurulum

### ayarlar ;

`NetCaseStudy.Api/appsettings.json` içinde:

* `ConnectionStrings:DefaultConnection` → SQL server bağlantım mevcut, macbook kullandığım için ben docker ile sql server'i kaldırıyorum. 
* `ConnectionStrings:Redis` → Redis bağlantım localhost:6379
* Serilog altında dosya yolu/format vs. (loglar ise NetCaseStudy.Api/Logs altına düşüyor)

### Database migration & seed

```bash

#migration oluşturuyoruz
dotnet ef migrations add InitialCreate \
  -p NetCaseStudy.Infrastructure \
  -s NetCaseStudy.Api \
  -c ApplicationDbContext \
  -o Persistence/Migrations

# güncelliyoruz
dotnet ef database update \
  -p NetCaseStudy.Infrastructure \
  -s NetCaseStudy.Api \
  -c ApplicationDbContext
```

> İlk çalıştırmada Identity ve örnek veriler seed ediliyor ( NetCaseStudy.Infrastructure projesi altındaki IdentitySeed dosyasında)

## Endpoint’ler (özet)

**Auth**

```http
POST /api/auth/register
POST /api/auth/login  // JWT dönüyor bununla da authorize
```

**Products**

```http
GET    /api/v1/products  // sayfalama/filtre/sort + ETag
GET    /api/v1/products/cursor  // cursor-based pagination
GET    /api/v1/products/{id}
POST   /api/v1/products  // Admin
PUT    /api/v1/products/{id} // Admin
DELETE /api/v1/products/{id}  // Admin
```

**Orders**

```http
GET    /api/v1/orders  // kullanıcı kendi siparişlerini, Admin hepsini görür
GET    /api/v1/orders/{id} // policy/resource-based (ViewOrder)
POST   /api/v1/orders  // ürün id + adet
PUT    /api/v1/orders/{id}/cancel // shipped değilse iptal
```

**Health**

```http
GET /health
```

---

## curl ile 

**Kayıt**

```bash
curl -X POST https://localhost:7247/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"enes@gmail.com","password":"123456!!"}'
```

**Giriş (JWT alıyoruz)**

```bash
curl -X POST https://localhost:7247/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"enes@gmail.com","password":"123456!!"}'
```

**Products listesi (ETag)**

```bash
# 1. e tag ı alıyorum.
curl -i "https://localhost:7247/api/v1/products?page=1&pageSize=10"

örneğin buradan dönen veri şu şekilde ; 

etag: "MqVJzgKBOmAj4caaUX1w02Jf17g0NjVl2D0EYy9dcws="

# 2. aldığım etag ile 304 bekliyorm
curl -i "https://localhost:7247/api/v1/products?page=1&pageSize=10" \
  -H 'If-None-Match: "MqVJzgKBOmAj4caaUX1w02Jf17g0NjVl2D0EYy9dcws"'
```

**Localization denemesi**

```bash
curl -i "https://localhost:7247/api/v1/products" -H "Accept-Language: tr-TR"
curl -i "https://localhost:7247/api/v1/products" -H "Accept-Language: en-US"
```

---

## Testler

* **Test**: xUnit kullandım.
* **Kapsam**: ağırlık **unit test**, kritik akışlarda **integration test** de var (Auth, Products, Orders + Health).
  Yardımcılar: `CustomWebApplicationFactory`, `FakeCacheService`, `FakeCurrentUserService`, `SelfHealthCheck`, `TestDataSeeder` vb.

---

## Bonus ;

* ✅ **ETag & 304** (ürün listesi)
* ✅ **Localization** (tr-TR / en-US)
* ✅ **Role-based rate limiting** (Admin serbest, User 50/dk gibi)
* ✅ **Cursor-based pagination**
* ✅ **Redis distributed cache** + invalidation
* ✅ **Policy/resource-based authorization** (ViewOrder)

---

## Bonus harici ek ;

* **API Versioning + Swagger/OpenAPI** (v1)
* **Health Checks** (SQL + Redis)
* **Serilog ile gelişmiş loglama** (structured, dosyaya yazan)
* **FluentValidation, MediatR, AutoMapper, Identity** altyapıları
