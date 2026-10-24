class ApiError {
  final String? code;
  final dynamic message;

  ApiError({
    required this.code,
    required this.message,
  });

  String get errorMessage => message is String
      ? message
      : message is List
          ? message.join('\n')
          : 'Unknown error';

  factory ApiError.fromJson(Map<String, dynamic> json) {
    return ApiError(
      code: json['code']?.toString(),
      message: json['message'],
    );
  }
}
