class PurchaseMiner {
  final String? count;
  final String? id;
  final String? name;
  final String? image;
  final String? purchasePrice;
  final String? dayRevenue;
  final String? depreciationCycle;
  final String? purchaseTotal;
  final String? status;
  final String? pNum;
  final String? gNum;
  final String? pCycle;
  final String? gCycle;
  final String? miningCurrency;
  final String? hashRate;
  final String? powerConsumption;
  final String? dayElecFee;
  final String? custodyFee;
  final String? custodyFeeRate;
  final String? tag;
  final String? tagColor;

  PurchaseMiner({
    this.count,
    this.id,
    this.name,
    this.image,
    this.purchasePrice,
    this.dayRevenue,
    this.depreciationCycle,
    this.purchaseTotal,
    this.status,
    this.pNum,
    this.gNum,
    this.pCycle,
    this.gCycle,
    this.miningCurrency,
    this.hashRate,
    this.powerConsumption,
    this.dayElecFee,
    this.custodyFee,
    this.custodyFeeRate,
    this.tag,
    this.tagColor,
  });

  factory PurchaseMiner.fromJson(Map<String, dynamic> json) {
    return PurchaseMiner(
      count: json['count']?.toString(),
      id: json['id']?.toString(),
      name: json['name']?.toString(),
      image: json['image']?.toString(),
      purchasePrice: json['purchase_price']?.toString(),
      dayRevenue: json['day_revenue']?.toString(),
      depreciationCycle: json['depreciation_cycle']?.toString(),
      purchaseTotal: json['purchase_total']?.toString(),
      status: json['status']?.toString(),
      pNum: json['p_num']?.toString(),
      gNum: json['g_num']?.toString(),
      pCycle: json['p_cycle']?.toString(),
      gCycle: json['g_cycle']?.toString(),
      miningCurrency: json['mining_currency']?.toString(),
      hashRate: json['hash_rate']?.toString(),
      powerConsumption: json['power_consumption']?.toString(),
      dayElecFee: json['day_elec_fee']?.toString(),
      custodyFee: json['custody_fee']?.toString(),
      custodyFeeRate: json['custody_fee_rate']?.toString(),
      tag: json['tag']?.toString(),
      tagColor: json['tag_color']?.toString(),
    );
  }
}
