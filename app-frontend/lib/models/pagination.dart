class Pagination {
  final String? perPage;
  final String? total;
  final String? pages;
  final String? previous;
  final String? current;
  final String? next;

  Pagination({
    this.perPage,
    this.total,
    this.pages,
    this.previous,
    this.current,
    this.next,
  });

  factory Pagination.fromJson(Map<String, dynamic> json) {
    return Pagination(
      perPage: json['per_page']?.toString(),
      total: json['total']?.toString(),
      pages: json['pages']?.toString(),
      previous: json['previous']?.toString(),
      current: json['current']?.toString(),
      next: json['next']?.toString(),
    );
  }
}
