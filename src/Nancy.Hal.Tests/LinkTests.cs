using Xunit;

namespace Nancy.Hal.Tests {
	public class LinkTests {
		[Fact]
		public void A_link_can_be_created() {
			var sut = new Link("self", "/some/uri", "some-title");
			Assert.Equal(sut.Title, "some-title");
			Assert.Equal(sut.Rel, "self");
			Assert.Equal(sut.Href, "/some/uri");
		}

		[Fact]
		public void A_templated_link_can_be_reated() {
			var sut = new Link("self", "/some/uri/{id}", "some-title");
			Assert.Equal(sut.Title, "some-title");
			Assert.Equal(sut.Rel, "self");
			Assert.Equal(sut.Href, "/some/uri/{id}");
		}

		[Fact]
		public void Using_a_templated_link_should_copy_all_fields() {
			var template = new Link("self", "/some/uri/{id}", "some-title");
			var sut = template.CreateLink(new { id = 1 });
			Assert.Equal(sut.Title, "some-title");
			Assert.Equal(sut.Rel, "self");
			Assert.Equal(sut.Href, "/some/uri/1");
		}

		[Fact]
		public void Using_a_templated_link_with_a_new_rel_should_have_the_new_rel() {
			var template = new Link("self", "/some/uri/{id}", "some-title");
			var sut = template.CreateLink("myrel", new { id = 1 });
			Assert.Equal(sut.Title, "some-title");
			Assert.Equal(sut.Rel, "myrel");
			Assert.Equal(sut.Href, "/some/uri/1");
		}

		[Fact]
		public void Using_a_templated_link_a_new_link_should_be_able_to_be_generated_with_title_and_rel() {
			var template = new Link("self", "/some/uri/{id}", "some-title");
			var sut = template.CreateLink("My Title", "myrel", new { id = 1 });
			Assert.Equal(sut.Title, "My Title");
			Assert.Equal(sut.Rel, "myrel");
			Assert.Equal(sut.Href, "/some/uri/1");
		}

        [Fact]
        public void Using_a_templated_link_without_a_title_should_copy_the_title_from_the_template_()
        {
            var template = new Link("self", "/some/uri/{id}", "some-title");
            var sut = template.CreateLink("myrel", new { id = 1 });
            Assert.Equal("some-title", sut.Title);
            Assert.Equal("myrel", sut.Rel);
            Assert.Equal("/some/uri/1", sut.Href);
        }
    }
}
