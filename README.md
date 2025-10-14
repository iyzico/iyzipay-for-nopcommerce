# iyzipay for nopCommerce

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![nopCommerce](https://img.shields.io/badge/nopCommerce-4.80%20%7C%204.90-green.svg)](https://www.nopcommerce.com/)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

Official iyzipay payment gateway integration plugin for nopCommerce e-commerce platform. This plugin enables secure payment processing through iyzipay's CheckoutForm API with support for 3D Secure, installment options, and webhook notifications.

## Features

- ✅ **CheckoutForm Integration** - Seamless payment form integration
- ✅ **3D Secure Support** - Enhanced security for card payments
- ✅ **Installment Options** - Flexible payment terms for customers
- ✅ **Webhook Support** - Real-time payment status updates
- ✅ **Multi-language Support** - Turkish and English localization
- ✅ **Sandbox & Live Environment** - Easy testing and production deployment
- ✅ **Order Status Management** - Automatic order status updates
- ✅ **Additional Fee Support** - Configurable payment processing fees

## Requirements

- nopCommerce 4.80 or 4.90
- .NET 9.0
- iyzipay merchant account
- SSL certificate (required for production)

## Installation

### Method 1: Binary Installation (Recommended)

1. **Download the plugin**
   ```bash
   git clone https://github.com/iyzico/iyzipay-for-nopcommerce.git
   ```

2. **Copy the plugin files**
   - Navigate to the `Payments.Iyzipay` directory in the downloaded repository
   - Copy the entire `Payments.Iyzipay` folder to your nopCommerce `Plugins` directory:
     ```
     /YourNopCommerceSite/Plugins/Payments.Iyzipay/
     ```

3. **Restart your nopCommerce application**
   - The plugin will be automatically detected and available for configuration

### Method 2: Source Code Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/iyzico/iyzipay-for-nopcommerce.git
   ```

2. **Build the plugin**
   ```bash
   cd iyzipay-for-nopcommerce/Iyzipay/nopCommerce-4.80/Nop.Plugin.Payments.Iyzipay
   dotnet build
   ```

3. **Copy the output**
   - The compiled plugin will be automatically copied to the nopCommerce Plugins directory
   - Restart your nopCommerce application

## Configuration

### 1. Enable the Plugin

1. Log in to your nopCommerce admin panel
2. Navigate to **Configuration → Local Plugins**
3. Find "Iyzipay CheckoutForm" in the payment methods section
4. Click **Install** and then **Edit**

### 2. Configure Payment Settings

#### Basic Configuration
- **API Key**: Your iyzipay API key
- **Secret Key**: Your iyzipay secret key
- **Environment**: 
  - Sandbox (for testing)
  - Live (for production)

#### Advanced Settings
- **Webhook URL**: `https://yourdomain.com/Plugins/PaymentIyzipay/Webhook`
- **Order Status After Payment**: Choose the order status after successful payment
- **Payment Form Language**: Turkish (tr) or English (en)
- **3D Secure**: Enable/disable 3D Secure authentication
- **Installment Options**: Configure maximum installment count
- **Additional Fee**: Set processing fees (fixed amount or percentage)

### 3. Test Configuration

1. **Sandbox Testing**
   - Set environment to "Sandbox"
   - Use test API credentials provided by iyzipay
   - Test with sample card numbers

2. **Production Setup**
   - Set environment to "Live"
   - Use your production API credentials
   - Ensure SSL certificate is properly configured

## Webhook Configuration

The plugin supports webhook notifications for real-time payment status updates:

1. **Configure webhook URL** in iyzipay merchant panel:
   ```
   https://yourdomain.com/Plugins/PaymentIyzipay/Webhook
   ```

2. **Enable webhook events**:
   - Payment success
   - Payment failure
   - Refund notifications

## Supported Payment Methods

- Credit/Debit Cards (Visa, MasterCard, American Express)
- 3D Secure authentication
- Installment payments (up to 12 installments)
- International cards

## Localization

The plugin includes localization files for:
- **Turkish (tr-TR)**: Complete translation
- **English (en-US)**: Complete translation

## Troubleshooting

### Common Issues

1. **Plugin not appearing**
   - Ensure the plugin files are in the correct directory
   - Check file permissions
   - Restart the application

2. **Payment failures**
   - Verify API credentials
   - Check SSL certificate
   - Review webhook configuration

3. **3D Secure issues**
   - Ensure proper SSL configuration
   - Check browser compatibility
   - Verify merchant configuration

### Debug Mode

Enable debug logging in nopCommerce to troubleshoot issues:
1. Navigate to **Configuration → Settings → Log**
2. Enable debug logging
3. Check log files for detailed error information

## Contributing

We welcome contributions to improve this plugin! Here's how you can help:

### How to Contribute

1. **Fork the repository**
   ```bash
   git clone https://github.com/your-username/iyzipay-for-nopcommerce.git
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow the existing code style
   - Add appropriate comments
   - Update documentation if needed

4. **Test your changes**
   - Test in both sandbox and live environments
   - Verify all payment flows work correctly
   - Check for any breaking changes

5. **Submit a pull request**
   - Provide a clear description of your changes
   - Include test results
   - Reference any related issues

### Development Setup

1. **Prerequisites**
   - Visual Studio 2022 or VS Code
   - .NET 9.0 SDK
   - nopCommerce 4.80+ source code

2. **Build the project**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run tests**
   ```bash
   dotnet test
   ```

### Code Style Guidelines

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public methods
- Include error handling and logging
- Write unit tests for new features

### Reporting Issues

When reporting issues, please include:
- nopCommerce version
- Plugin version
- Error messages and logs
- Steps to reproduce
- Expected vs actual behavior

## Support

### Community Support

- **GitHub Issues**: [Report bugs and request features](https://github.com/iyzico/iyzipay-for-nopcommerce/issues)
- **Discussions**: [Community discussions](https://github.com/iyzico/iyzipay-for-nopcommerce/discussions)

### Commercial Support

For commercial support and custom development:
- **Iyzico Support**: [support@iyzico.com](mailto:support@iyzico.com)
- **Documentation**: [iyzipay Documentation](https://dev.iyzipay.com/)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

### Version 4.90.1
- Updated for nopCommerce 4.90 compatibility
- Enhanced webhook processing
- Improved error handling
- Added additional fee configuration options

### Version 4.80.1
- Initial release for nopCommerce 4.80
- Basic CheckoutForm integration
- 3D Secure support
- Installment options

## Acknowledgments

- [nopCommerce](https://www.nopcommerce.com/) - The e-commerce platform
- [iyzipay](https://www.iyzico.com/) - The payment gateway
- Contributors and community members

---

**Made with ❤️ by [Iyzico](https://www.iyzico.com/)**
