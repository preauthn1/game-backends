class HomeInformation {
  final List<TopSellingMiner>? topSellingMiners;
  final List<TopSalesPeople>? topSalesPeople;

  HomeInformation({
    required this.topSellingMiners,
    required this.topSalesPeople,
  });

  factory HomeInformation.fromJson(Map<String, dynamic> json) {
    return HomeInformation(
      topSellingMiners: json['top_selling_miners'] != null
          ? List<TopSellingMiner>.from(
              json['top_selling_miners'].map(
                (x) => TopSellingMiner.fromJson(x),
              ),
            )
          : null,
      topSalesPeople: json['top_sales_people'] != null
          ? List<TopSalesPeople>.from(
              json['top_sales_people'].map(
                (x) => TopSalesPeople.fromJson(x),
              ),
            )
          : null,
    );
  }
}

class TopSellingMiner {
  final String? name;
  final String? amount;
  final String? info;
  final String? image;

  TopSellingMiner({
    required this.name,
    required this.amount,
    required example.invalid,
    required this.image,
  });

  factory TopSellingMiner.fromJson(Map<String, dynamic> json) {
    return TopSellingMiner(
      name: json['name']?.toString(),
      amount: json['amount']?.toString(),
      info: json['info']?.toString(),
      image: json['image']?.toString(),
    );
  }
}

class TopSalesPeople {
  final String? username;
  final String? image;
  final String? quantitySold;
  final String? rebateAmount;

  TopSalesPeople({
    required this.username,
    required this.image,
    required this.quantitySold,
    required this.rebateAmount,
  });

  factory TopSalesPeople.fromJson(Map<String, dynamic> json) {
    return TopSalesPeople(
      username: json['username']?.toString(),
      image: json['image']?.toString(),
      quantitySold: json['quantity_sold']?.toString(),
      rebateAmount: json['rebate_amount']?.toString(),
    );
  }
}
