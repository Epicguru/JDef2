namespace JXml.Utils
{
    public enum ListMergeMode
    {
        /// <summary>
        /// The values in the new list are added to the old list.
        /// Duplicate items are discarded.
        /// <para>[A, B, C] + [B] => [A, B, C]</para>
        /// </summary>
        Merge,

        /// <summary>
        /// The values in the new list are added to the old list.
        /// If there are duplicate items,
        /// the value from the new list replaces the old value.
        /// <para>[A, B, C] + [B'] => [A, B', C]</para>
        /// Duplicate items are items (a, b) where:
        /// <code>a.equals(b)</code>
        /// </summary>
        MergeReplace,

        /// <summary>
        /// The values in the new list are added to the old list.
        /// Items may included more than once.
        /// <para>[A, B, C] + [B] => [A, B, C, B]</para>
        /// </summary>
        Append,

        /// <summary>
        /// The values in the new list replace the values from the old list.
        /// <para>[A, B, C] + [B] => [B]</para>
        /// </summary>
        Replace,

        /// <summary>
        /// The values in the new list subtract from the values in the old list.
        /// <para>[A, B, C] + [B] => [A, C]</para>
        /// </summary>
        Subtract
    }
}
