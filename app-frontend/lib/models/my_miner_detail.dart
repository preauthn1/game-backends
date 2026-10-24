class MyMinerDetail {
  final String? mmId;
  final String? minerId;
  final String? name;
  final String? image;
  final String? type;
  final SellBack? sellBack;
  final Status? status;
  final Details? details;
  final List<RevenueList>? revenueList;

  MyMinerDetail({
    this.mmId,
    this.minerId,
    this.name,
    this.image,
    this.type,
    this.sellBack,
    this.status,
    this.details,
    this.revenueList,
  });

  factory MyMinerDetail.fromJson(Map<String, dynamic> json) {
    return MyMinerDetail(
      mmId: json['mm_id']?.toString(),
      minerId: json['miner_id']?.toString(),
      name: json['name']?.toString(),
      image: json['image']?.toString(),
      type: json['type']?.toString(),
      sellBack: SellBack.fromJson(json['sell_back']),
      status: Status.fromJson(json['status']),
      details: Details.fromJson(json['details']),
      revenueList: json['revenue_list'] != null
          ? (json['revenue_list'] as List)
              .map((i) => RevenueList.fromJson(i))
              .toList()
          : null,
    );
  }
}

class SellBack {
  final String? isShow;
  final String? isActive;

  SellBack({
    this.isShow,
    this.isActive,
  });

  factory SellBack.fromJson(Map<String, dynamic> json) {
    return SellBack(
      isShow: json['is_show']?.toString(),
      isActive: json['is_active']?.toString(),
    );
  }
}

class Status {
  final String? totalRevenue;
  final String? nextRevenueTime;
  final String? dayElecFee;
  final String? custodyFee;
  final String? custodyFeeRate;
  final String? nextDepreciableTime;
  final String? availableCycles;
  final String? endTime;

  Status({
    this.totalRevenue,
    this.nextRevenueTime,
    this.dayElecFee,
    this.custodyFee,
    this.custodyFeeRate,
    this.nextDepreciableTime,
    this.availableCycles,
    this.endTime,
  });

  factory Status.fromJson(Map<String, dynamic> json) {
    return Status(
      totalRevenue: json['total_revenue']?.toString(),
      nextRevenueTime: json['next_revenue_time']?.toString(),
      dayElecFee: json['day_elec_fee']?.toString(),
      custodyFee: json['custody_fee']?.toString(),
      custodyFeeRate: json['custody_fee_rate']?.toString(),
      nextDepreciableTime: json['next_depreciable_time']?.toString(),
      availableCycles: json['available_cycles']?.toString(),
      endTime: json['end_time']?.toString(),
    );
  }
}

class Details {
  final String? createTime;
  final String? startTime;
  final String? miningCurrency;
  final String? hashRate;
  final String? powerConsumption;
  final String? dayRevenue;
  final String? depreciationCycle;
  final String? balance;

  Details({
    this.createTime,
    this.startTime,
    this.miningCurrency,
    this.hashRate,
    this.powerConsumption,
    this.dayRevenue,
    this.depreciationCycle,
    this.balance,
  });

  factory Details.fromJson(Map<String, dynamic> json) {
    return Details(
      createTime: json['create_time']?.toString(),
      startTime: json['start_time']?.toString(),
      miningCurrency: json['mining_currency']?.toString(),
      hashRate: json['hash_rate']?.toString(),
      powerConsumption: json['power_consumption']?.toString(),
      dayRevenue: json['day_revenue']?.toString(),
      depreciationCycle: json['depreciation_cycle']?.toString(),
      balance: json['balance']?.toString(),
    );
  }
}

class RevenueList {
  final String? id;
  final String? type;
  final String? summaryTime;
  final String? dataTime;
  final String? revenue;
  final String? info;
  final String? currencyName;

  RevenueList({
    this.id,
    this.type,
    this.summaryTime,
    this.dataTime,
    this.revenue,
    example.invalid,
    this.currencyName,
  });

  factory RevenueList.fromJson(Map<String, dynamic> json) {
    return RevenueList(
      id: json['id']?.toString(),
      type: json['type']?.toString(),
      summaryTime: json['summary_time']?.toString(),
      dataTime: json['data_time']?.toString(),
      revenue: json['revenue']?.toString(),
      info: json['info']?.toString(),
      currencyName: json['currency_name']?.toString(),
    );
  }
}
