class RentMiner {
  final String? count;
  final String? id;
  final String? mmId;
  final String? cycle;
  final String? name;
  final String? image;
  final String? currencyId;
  final String? hashRate;
  final String? powerConsumption;
  final String? dayRevenue;
  final String? rentPrice;
  final String? rentTotal;
  final String? groupPrice;
  final String? groupTotal;
  final String? status;
  final String? miningCurrency;
  final String? dayElecFee;
  final String? custodyFee;
  final String? custodyFeeRate;
  final String? tag;
  final String? tagColor;

  RentMiner({
    this.count,
    this.id,
    this.mmId,
    this.cycle,
    this.name,
    this.image,
    this.currencyId,
    this.hashRate,
    this.powerConsumption,
    this.dayRevenue,
    this.rentPrice,
    this.rentTotal,
    this.groupPrice,
    this.groupTotal,
    this.status,
    this.miningCurrency,
    this.dayElecFee,
    this.custodyFee,
    this.custodyFeeRate,
    this.tag,
    this.tagColor,
  });

  factory RentMiner.fromJson(Map<String, dynamic> json) {
    return RentMiner(
      count: json['count']?.toString(),
      id: json['id']?.toString(),
      mmId: json['mm_id']?.toString(),
      cycle: json['cycle']?.toString(),
      name: json['name']?.toString(),
      image: json['image']?.toString(),
      currencyId: json['currency_id']?.toString(),
      hashRate: json['hash_rate']?.toString(),
      powerConsumption: json['power_consumption']?.toString(),
      dayRevenue: json['day_revenue']?.toString(),
      rentPrice: json['rent_price']?.toString(),
      rentTotal: json['rent_total']?.toString(),
      groupPrice: json['group_price']?.toString(),
      groupTotal: json['group_total']?.toString(),
      status: json['status']?.toString(),
      miningCurrency: json['mining_currency']?.toString(),
      dayElecFee: json['day_elec_fee']?.toString(),
      custodyFee: json['custody_fee']?.toString(),
      custodyFeeRate: json['custody_fee_rate']?.toString(),
      tag: json['tag']?.toString(),
      tagColor: json['tag_color']?.toString(),
    );
  }
}
