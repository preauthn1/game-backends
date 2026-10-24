class Member {
  String? id;
  String? uid;
  String? username;
  String? realName;
  String? avatarImage;
  String? email;
  String? countryCode;
  String? mobile;
  String? mobileValidate;
  String? fundPasswordStatus;
  String? totalProceeds;
  String? todayEarning;
  String? idInformationStatus;
  String? idInformationRemark;
  String? is2faEnable;
  MemberBank? bank;
  MemberCrypto? crypto;

  Member({
    this.id,
    this.uid,
    this.username,
    this.realName,
    this.avatarImage,
    this.email,
    this.countryCode,
    this.mobile,
    this.mobileValidate,
    this.fundPasswordStatus,
    this.totalProceeds,
    this.todayEarning,
    this.idInformationStatus,
    this.idInformationRemark,
    this.is2faEnable,
    this.bank,
    this.crypto,
  });

  factory Member.fromJson(Map<String, dynamic> json) {
    return Member(
      id: json['id']?.toString(),
      uid: json['uid']?.toString(),
      username: json['username']?.toString(),
      realName: json['real_name']?.toString(),
      avatarImage: json['avatar_image']?.toString(),
      email: json['email']?.toString(),
      countryCode: json['country_code']?.toString(),
      mobile: json['mobile']?.toString(),
      mobileValidate: json['mobile_validate']?.toString(),
      fundPasswordStatus: json['fund_password_status']?.toString(),
      totalProceeds: json['total_proceeds']?.toString(),
      todayEarning: json['today_earning']?.toString(),
      idInformationStatus: json['id_information_status']?.toString(),
      idInformationRemark: json['id_information_remark']?.toString(),
      is2faEnable: json['is_2fa_enable']?.toString(),
      bank: MemberBank.fromJson(json['bank']),
      crypto: MemberCrypto.fromJson(json['crypto']),
    );
  }
}

class MemberBank {
  String? status;
  String? id;
  String? name;
  String? account;
  String? accountName;
  String? image;

  MemberBank({
    required this.status,
    required this.id,
    required this.name,
    required this.account,
    required this.accountName,
    required this.image,
  });

  factory MemberBank.fromJson(Map<String, dynamic> json) {
    return MemberBank(
      status: json['status']?.toString(),
      id: json['id']?.toString(),
      name: json['name']?.toString(),
      account: json['account']?.toString(),
      accountName: json['account_name']?.toString(),
      image: json['image']?.toString(),
    );
  }
}

class MemberCrypto {
  String? status;
  String? id;
  String? name;
  String? address;
  String? accountName;
  String? image;

  MemberCrypto({
    required this.status,
    required this.id,
    required this.name,
    required this.address,
    required this.accountName,
    required this.image,
  });

  factory MemberCrypto.fromJson(Map<String, dynamic> json) {
    return MemberCrypto(
      status: json['status']?.toString(),
      id: json['id']?.toString(),
      name: json['name']?.toString(),
      address: json['address']?.toString(),
      accountName: json['account_name']?.toString(),
      image: json['image']?.toString(),
    );
  }
}
