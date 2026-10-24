class Promotion {
  final String? id;
  final String? type;
  final String? title;
  final String? content;
  final String? image;
  final String? imageContent;
  final String? url;

  // type; 0:預設(跳轉url), 1:登入頁彈窗, 2:內頁彈窗
  Promotion({
    this.id,
    this.type,
    this.title,
    this.content,
    this.image,
    this.imageContent,
    this.url,
  });

  factory Promotion.fromJson(Map<String, dynamic> json) {
    return Promotion(
      id: json['id']?.toString(),
      type: json['type']?.toString(),
      title: json['title']?.toString(),
      content: json['content']?.toString(),
      image: json['image']?.toString(),
      imageContent: json['image_content']?.toString(),
      url: json['url']?.toString(),
    );
  }
}
